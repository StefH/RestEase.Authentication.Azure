using AnyOfTypes.Newtonsoft.Json;
using ConsoleAppExample.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace ConsoleAppExample;

static class Program
{
    static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(theme: AnsiConsoleTheme.Code)
            .CreateLogger();

        await using ServiceProvider serviceProvider = RegisterServices(args);

        Worker worker = serviceProvider.GetRequiredService<Worker>();

        await worker.RunAsync(CancellationToken.None);
    }

    private static ServiceProvider RegisterServices(string[] args)
    {
        IConfiguration configuration = SetupConfiguration(args);
        var services = new ServiceCollection();

        services.AddSingleton(configuration);

        services.AddLogging(builder => builder.AddSerilog(logger: Log.Logger, dispose: true));

        services
            .UseWithAzureAuthenticatedRestEaseClient<IDocumentApi>(
                configuration.GetSection("DocumentApiClientOptions"),
                c => c.JsonSerializerSettings = new JsonSerializerSettings { Converters = new List<Newtonsoft.Json.JsonConverter> { new AnyOfJsonConverter() } });

        services.AddSingleton<Worker>();

        return services.BuildServiceProvider();
    }

    private static IConfiguration SetupConfiguration(string[] args)
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();
    }
}