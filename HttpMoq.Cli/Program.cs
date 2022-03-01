using HttpMoq;
using HttpMoq.Cli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var cliConfig = new ConfigurationBuilder()
    .AddCommandLine(args)
    .Build();

var configPath = "httpmoq.json";
if (!string.IsNullOrEmpty(cliConfig["config"]))
{
    configPath = cliConfig["config"];
}

var currentDirectory = Directory.GetCurrentDirectory();
configPath = Path.Combine(currentDirectory, configPath);
var configuration = new ConfigurationBuilder()
    .AddJsonFile(configPath, false)
    .AddCommandLine(args)
    .Build();

var services = new ServiceCollection()
    .AddLogging(c =>
    {
        c.SetMinimumLevel(cliConfig.GetValue<bool>("verbose") ? LogLevel.Debug : LogLevel.Information);
        c.AddSystemdConsole();
    })
    .BuildServiceProvider();

var logger = services.GetRequiredService<ILogger<Program>>();
var apiLogger = services.GetRequiredService<ILogger<MockApi>>();

var requests = new List<RequestModel>();
configuration.GetSection("requests").Bind(requests);

var exit = new ManualResetEvent(false);
var done = new ManualResetEvent(false);
var api = new MockApi(configuration.GetValue<int>("port"));

foreach (var request in requests)
{
    logger.LogDebug("Setting up mock request, for {0}: {1}", request.Method, request.Path);
    var req = api.Expect(request.Method, request.Path)
        .SetStatusCode(request.Response.StatusCode);

    if (request.Limit != null)
    {
        req.SetLimit((uint)request.Limit);
    }

    if (!string.IsNullOrEmpty(request.Response.Text))
    {
        req.ReturnText(request.Response.Text);
    }

    if (request.Response.Json != null)
    {
        req.ReturnJson(request.Response.Json);
    }
}

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    exit.Set();
    done.WaitOne();
};

logger.LogInformation("Starting MockApi...");

await api.StartAsync();

logger.LogInformation("Listening on {0}...", api.Url);

var logSubscriber = ApiLogSubscriber.Start(api, apiLogger);

exit.WaitOne();

logger.LogInformation("Stopping...");

await api.StopAsync();
logSubscriber.Stop();
done.Set();

logger.LogInformation("Stopped.");