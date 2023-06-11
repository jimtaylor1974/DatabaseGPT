using DatabaseGPT;
using DatabaseGPT.ConsoleApp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

Providers.RegisterDbProviderFactories();

var debugDirectory = new DirectoryInfo(AppContext.BaseDirectory)
    .Parent
    .Parent
    .Parent
    .FullName;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        // config.SetBasePath(Path.Combine(AppContext.BaseDirectory));
        config.SetBasePath(debugDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddUserSecrets("c7e4a900-3522-4fe7-adee-a488aaef2ea2", reloadOnChange: true);
    })
    .ConfigureLogging((hostBuilderContext, logging) =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    })
    .ConfigureServices((hostBuilderContext, services) =>
    {
        var configuration = hostBuilderContext.Configuration;
        var appSettingsSection = configuration.GetSection("AppSettings");
        services.Configure<AppSettings>(appSettingsSection);
        services.AddSingleton<ConsoleAppService>();
        ConfigurationUtility.ConfigureServices(services, configuration);
    })
    .Build();

var services = host.Services;
var consoleAppService = services.GetService<ConsoleAppService>()!;

var options = new[]
{
    new Option
    {
        Key = "-test",
        Title = "Test code",
        Func = consoleAppService.TestAsync
    },
};

var cancellationToken = CancellationToken.None;

if (args.Length != 0)
{
    var primaryArgument = args[0];

    var command = options.FirstOrDefault(o => o.Key == primaryArgument);

    if (command != null)
    {
        await command.Func(args.Skip(1).ToArray(), cancellationToken);
        return;
    }
}

bool exit = false;

while (!exit)
{
    foreach (var option in options.Select((a, i) => new { action = a.Title, number = i + 1 }))
    {
        Console.WriteLine("Enter {0} to {1}", option.number, option.action);
    }

    Console.WriteLine("Or anything else to exit.");

    var number = int.TryParse(Console.ReadLine(), out var result) ? (int?)result : null;
    var selectedOption = options.ElementAtOrDefault(number.GetValueOrDefault(-1) - 1);

    if (selectedOption == null)
    {
        exit = true;
    }
    else
    {
        try
        {
            await selectedOption.Func(args, cancellationToken);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }
}

class Option
{
    public string Key { get; set; }
    public string Title { get; set; }
    public Func<string[], CancellationToken, Task> Func { get; set; }
}
