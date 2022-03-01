using Microsoft.Extensions.Logging;

namespace HttpMoq.Cli;

internal class ApiLogSubscriber
{
    private readonly CancellationTokenSource _ctx;
    private readonly MockApi _api;
    private readonly ILogger<MockApi> _logger;
    private Thread _reader;

    private ApiLogSubscriber(MockApi api, ILogger<MockApi> logger)
    {
        _ctx = new CancellationTokenSource();
        _api = api;
        _logger = logger;
    }

    public void Start()
    {
        _reader = new Thread(() =>
        {
            while (!_ctx.IsCancellationRequested)
            {
                _api.PrintOutput(msg => _logger.LogInformation(msg));
                Task.Delay(250).Wait();
            }
        });
        _reader.Start();
    }

    public static ApiLogSubscriber Start(MockApi api, ILogger<MockApi> logger)
    {
        var subscriber = new ApiLogSubscriber(api, logger);
        subscriber.Start();
        return subscriber;
    }

    public void Stop()
    {
        _ctx.Cancel();
        _reader.Join();
    }
}