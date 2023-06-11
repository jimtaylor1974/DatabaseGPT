using DatabaseGPT.DatabaseSchema;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Text;
using DatabaseGPT.Infrastructure.Extensions;
using Flurl.Http;
using Microsoft.Extensions.Options;

namespace DatabaseGPT;

public class QueryService
{
    private const string CompletionsApiUrl = "https://api.openai.com/v1/completions";
    private const string Model = "text-davinci-003";
    private const int MaxTokens = 800;

    private readonly SqlDatabaseSchemaService sqlDatabaseSchemaService;
    private readonly IConfiguration configuration;
    private readonly string openAiApiKey;

    public QueryService(SqlDatabaseSchemaService sqlDatabaseSchemaService, IConfiguration configuration, IOptionsSnapshot<AppSettings> appSettings)
    {
        this.sqlDatabaseSchemaService = sqlDatabaseSchemaService;
        this.configuration = configuration;
        this.openAiApiKey = appSettings.Value.OpenAiApiKey;
    }

    public async Task<string> GetSchemaPromptAsync(string connectionStringName)
    {
        var cacheFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "SQL_SCHEMA_INFO", $"SchemaPrompt_{connectionStringName}.txt");

        if (File.Exists(cacheFilePath))
        {
            return await File.ReadAllTextAsync(cacheFilePath);
        }

        var connectionString = configuration.GetConnectionString(connectionStringName);
        var schema = sqlDatabaseSchemaService.Import(Providers.SqlClient, connectionString);

        var sb = new StringBuilder();

        foreach (var table in schema.TableDefinitions)
        {
            sb.AppendLine($"Schema for table: [{table.schema}].[{table.name}]");

            foreach (var column in table.columns)
            {
                sb.AppendLine($"\t{column.name} {column.type}");
            }

            sb.AppendLine();

            sb.AppendLine($"Data for table: {table.schema}.{table.name}:");

            string query = $"SELECT TOP 5 * FROM [{table.schema}].[{table.name}]";
            var data = await ExecuteQueryAndDisplayDataAsync(connectionString, query);

            sb.Append(data);

            sb.AppendLine();
        }

        await File.WriteAllTextAsync(cacheFilePath, sb.ToString());

        return sb.ToString();
    }

    public async Task<string> ExecuteQueryAndDisplayDataAsync(string connectionString, string query)
    {
        var sb = new StringBuilder();

        using SqlConnection connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        using SqlCommand command = new SqlCommand(query, connection);
        using SqlDataReader reader = await command.ExecuteReaderAsync();

        var data = new List<List<string>>();
        int fieldCount = reader.FieldCount;
        int[] maxLengths = new int[fieldCount];

        // Write the column names to the string builder
        var headerRow = new List<string>();
        for (int i = 0; i < fieldCount; i++)
        {
            headerRow.Add(reader.GetName(i));
            maxLengths[i] = Math.Max(maxLengths[i], reader.GetName(i).Length);
        }

        data.Add(headerRow);

        // Write the data to the string builder
        while (await reader.ReadAsync())
        {
            var row = new List<string>();

            for (int i = 0; i < fieldCount; i++)
            {
                maxLengths[i] = Math.Max(maxLengths[i], reader[i].ToString().Length);
                row.Add(reader[i].ToString());
            }

            data.Add(row);
        }

        foreach (var row in data)
        {
            for (int i = 0; i < fieldCount; i++)
            {
                sb.Append(row[i].PadRight(maxLengths[i] + 2));
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    public async Task<string> GetQueryTextAsync(string schemaPrompt, string question, ExecutionResult? previousExecutionResult = null)
    {
        var prompt = GeneratePrompt(schemaPrompt, question, previousExecutionResult);

        var gpt3Response = await GetGpt3ResponseAsync(prompt);

        return gpt3Response?.choices.FirstOrDefault()?.text ?? "";
    }

    private string GeneratePrompt(string schemaPrompt, string question, ExecutionResult? previousExecutionResult)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"{schemaPrompt}\nAs a senior analyst, given the above schemas and data, write a detailed and correct SQL SERVER query to answer the analytical question:\n\"{question}\"\nFully qualify the table names - use [ and ], and comment the query with your logic.");

        if (!string.IsNullOrEmpty(previousExecutionResult?.ExceptionText))
        {
            stringBuilder.AppendLine($"\nThe previously generated SQL SERVER query that was generated is\n==========\n{previousExecutionResult.QueryText}\n==========\nIt failed with the following exception:\n{previousExecutionResult.ExceptionText}\n\nPlease make sure that the generated SQL SERVER avoids the same problem.");
        }

        return stringBuilder.ToString();
    }

    private async Task<GPT3Response> GetGpt3ResponseAsync(string prompt)
    {
        var response = await SendRequestToOpenAiApiAsync(prompt);

        if (!response.StatusCode.IsSuccessStatusCode())
        {
            HandleOpenAiApiError(response);
            return null;
        }

        return await response.GetJsonAsync<GPT3Response>();
    }

    private async Task<IFlurlResponse> SendRequestToOpenAiApiAsync(string prompt)
    {
        return await CompletionsApiUrl
            .AllowAnyHttpStatus()
            .WithOAuthBearerToken(openAiApiKey)
            .PostJsonAsync(new
            {
                model = Model,
                prompt = prompt,
                temperature = 0,
                max_tokens = MaxTokens
            });
    }

    private async void HandleOpenAiApiError(IFlurlResponse response)
    {
        var content = await response.GetStringAsync();
        Console.WriteLine($"Call to {CompletionsApiUrl} failed with HTTP status code {response.StatusCode}, response text = {content}");
    }
}

public class ExecutionResult
{
    public ExecutionResult(string queryText)
    {
        QueryText = queryText;
    }

    public string QueryText { get; }
    public string ExceptionText { get; set; }
}

class GPT3Response
{
    public string id { get; set; }
    public string @object { get; set; }
    public int created { get; set; }
    public string model { get; set; }
    public List<Choice> choices { get; set; }
    public Usage usage { get; set; }
}

class Choice
{
    public string text { get; set; }
    public int index { get; set; }
    public object logprobs { get; set; }
    public string finish_reason { get; set; }
}

class Usage
{
    public int prompt_tokens { get; set; }
    public int completion_tokens { get; set; }
    public int total_tokens { get; set; }
}
