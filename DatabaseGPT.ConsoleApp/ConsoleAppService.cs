using Microsoft.Extensions.Configuration;

namespace DatabaseGPT.ConsoleApp;

internal class ConsoleAppService
{
    // REF: https://www.patterns.app/blog/2023/01/18/crunchbot-sql-analyst-gpt/
    private const string ConnectionStringName = "OurBudget"; // "AdventureWorks2016"; // "WorldWideImporters";
    private const int MaxRetries = 3;

    private readonly QueryService queryService;
    private readonly IConfiguration configuration;

    public ConsoleAppService(QueryService queryService, IConfiguration configuration)
    {
        this.queryService = queryService;
        this.configuration = configuration;
    }

    public async Task TestAsync(string[] args, CancellationToken cancellationToken)
    {
        var schemaPrompt = await queryService.GetSchemaPromptAsync(ConnectionStringName);

        while (true)
        {
            Console.WriteLine("Enter your question");

            var question = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(question))
            {
                break;
            }

            var retryCount = 0;
            ExecutionResult previousExecutionResult = null;

            while (retryCount < MaxRetries)
            {
                string queryText = null;
                try
                {
                    queryText = await queryService.GetQueryTextAsync(schemaPrompt, question, previousExecutionResult);

                    Console.WriteLine(queryText);
                    Console.WriteLine();

                    Console.WriteLine("Hit enter to execute query or anything else to try another query");
                    question = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(question))
                    {
                        // Start with a new question
                        break;
                    }

                    var connectionString = configuration.GetConnectionString(ConnectionStringName);
                    var data = await queryService.ExecuteQueryAndDisplayDataAsync(connectionString, queryText);

                    Console.WriteLine(data);
                    break; // break the loop if execution was successful
                }
                catch (Exception exception)
                {
                    previousExecutionResult = new ExecutionResult(queryText);
                    previousExecutionResult.ExceptionText = exception.ToString();

                    Console.WriteLine(previousExecutionResult.ExceptionText);

                    if (++retryCount >= MaxRetries)
                    {
                        Console.WriteLine("Maximum retry attempts exceeded.");
                        break;
                    }
                }
            }
        }
    }
}
