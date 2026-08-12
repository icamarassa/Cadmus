using System.Net.Http.Json;
using Cadmus.Collector.Contracts;

namespace Cadmus.Collector;

public sealed class Worker : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IHttpClientFactory httpClientFactory,
        ILogger<Worker> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Cadmus Collector iniciou em {StartedAt}",
            DateTimeOffset.Now);

        await SendTestEventAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Cadmus Collector está em execução em {CheckedAt}",
                DateTimeOffset.Now);

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task SendTestEventAsync(CancellationToken stoppingToken)
    {
        var printEvent = new CollectorPrintEventRequest
        {
            SourceEventId = "DEV-CADMUS-COLLECTOR:307:1",
            UserName = "cadmus.collector",
            DocumentName = "evento-de-teste.txt",
            PrinterName = "IMPRESSORA-TESTE",
            ClientComputer = Environment.MachineName,
            Pages = 1,
            Status = "Completed",
            CompletedAt = DateTimeOffset.UtcNow
        };

        var client = _httpClientFactory.CreateClient("CadmusApi");

        var response = await client.PostAsJsonAsync(
            "api/v1/collector/print-events",
            printEvent,
            stoppingToken);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation(
                "Evento de teste enviado. Status HTTP: {StatusCode}",
                (int)response.StatusCode);

            return;
        }

        var error = await response.Content.ReadAsStringAsync(stoppingToken);

        _logger.LogError(
            "Falha ao enviar evento de teste. Status HTTP: {StatusCode}. Erro: {Error}",
            (int)response.StatusCode,
            error);
    }
}