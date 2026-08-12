using System.Diagnostics.Eventing.Reader;
using System.Net.Http.Json;
using Cadmus.Collector.Contracts;

namespace Cadmus.Collector;

public sealed class Worker : BackgroundService
{
    private const string PrintServiceLogName =
        "Microsoft-Windows-PrintService/Operational";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IHttpClientFactory httpClientFactory,
        ILogger<Worker> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Cadmus Collector iniciou em {StartedAt}",
            DateTimeOffset.Now);

        const string query = "*[System[(EventID=307)]]";

        var eventQuery = new EventLogQuery(
            PrintServiceLogName,
            PathType.LogName,
            query)
        {
            ReverseDirection = true
        };

        using var reader = new EventLogReader(eventQuery);

        for (var count = 0; count < 10; count++)
        {
            using var printEvent = reader.ReadEvent();

            if (printEvent is null)
            {
                break;
            }

            if (printEvent.Properties.Count < 8)
            {
                _logger.LogWarning(
                    "Evento {RecordId} ignorado: formato inesperado.",
                    printEvent.RecordId);

                continue;
            }

            var printJob = new CollectorPrintEventRequest
            {
                SourceEventId =
                    $"{printEvent.MachineName}:{printEvent.RecordId}",
                DocumentName =
                    printEvent.Properties[1].Value?.ToString() ?? string.Empty,
                UserName =
                    printEvent.Properties[2].Value?.ToString() ?? string.Empty,
                ClientComputer =
                    printEvent.Properties[3].Value?.ToString(),
                PrinterName =
                    printEvent.Properties[4].Value?.ToString() ?? string.Empty,
                Pages = Convert.ToInt32(printEvent.Properties[7].Value),
                Status = "Completed",
                CompletedAt = printEvent.TimeCreated
            };

            await SendPrintEventAsync(printJob, stoppingToken);
        }
    }

    private async Task SendPrintEventAsync(
        CollectorPrintEventRequest printEvent,
        CancellationToken stoppingToken)
    {
        var client = _httpClientFactory.CreateClient("CadmusApi");

        var response = await client.PostAsJsonAsync(
            "api/v1/collector/print-events",
            printEvent,
            stoppingToken);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation(
                "Impressão enviada: {DocumentName} | {UserName} | {PrinterName}",
                printEvent.DocumentName,
                printEvent.UserName,
                printEvent.PrinterName);

            return;
        }

        var error = await response.Content.ReadAsStringAsync(stoppingToken);

        _logger.LogError(
            "Falha ao enviar evento {SourceEventId}. HTTP {StatusCode}: {Error}",
            printEvent.SourceEventId,
            (int)response.StatusCode,
            error);
    }
}