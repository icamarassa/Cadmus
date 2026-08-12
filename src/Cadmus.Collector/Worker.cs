namespace Cadmus.Collector;

public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Cadmus Collector iniciou em {StartedAt}",
            DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Cadmus Collector está em execução em {CheckedAt}",
                DateTimeOffset.Now);

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}