using System.Diagnostics.Eventing.Reader;
using System.Net.Http.Json;
using Cadmus.Collector.Contracts;
using Cadmus.Collector.State;

namespace Cadmus.Collector;

public sealed class Worker : BackgroundService
{
    private const string PrintServiceLogName =
        "Microsoft-Windows-PrintService/Operational";

    private static readonly TimeSpan PollingInterval =
        TimeSpan.FromSeconds(15);

    private readonly CollectorCheckpointStore _checkpointStore;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<Worker> _logger;

    public Worker(
        CollectorCheckpointStore checkpointStore,
        IHttpClientFactory httpClientFactory,
        ILogger<Worker> logger)
    {
        _checkpointStore = checkpointStore;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var lastRecordId = await _checkpointStore.GetLastRecordIdAsync(
            stoppingToken);

        _logger.LogInformation(
            "Cadmus Collector iniciou. Último evento processado: {RecordId}",
            lastRecordId);

        while (!stoppingToken.IsCancellationRequested)
        {
            lastRecordId = await ProcessNewEventsAsync(
                lastRecordId,
                stoppingToken);

            await Task.Delay(PollingInterval, stoppingToken);
        }
    }

    private async Task<long> ProcessNewEventsAsync(
        long lastRecordId,
        CancellationToken stoppingToken)
    {
        var query =
            $"*[System[(EventID=307) and (EventRecordID > {lastRecordId})]]";

        var eventQuery = new EventLogQuery(
            PrintServiceLogName,
            PathType.LogName,
            query);

        using var reader = new EventLogReader(eventQuery);

        while (!stoppingToken.IsCancellationRequested)
        {
            using var printEvent = reader.ReadEvent();

            if (printEvent is null)
            {
                break;
            }

            if (printEvent.RecordId is not long recordId)
            {
                continue;
            }

            if (printEvent.Properties.Count < 8)
            {
                _logger.LogWarning(
                    "Evento {RecordId} ignorado: formato inesperado.",
                    recordId);

                await SaveCheckpointAsync(recordId, stoppingToken);
                lastRecordId = recordId;
                continue;
            }

            var printJob = new CollectorPrintEventRequest
            {
                SourceEventId = $"{printEvent.MachineName}:{recordId}",
                DocumentName =
                    printEvent.Properties[1].Value?.ToString() ?? string.Empty,
                UserName =
                    printEvent.Properties[2].Value?.ToString() ?? string.Empty,
                ClientComputer =
                    printEvent.Properties[3].Value?.ToString(),
                PrinterName =
                    printEvent.Properties[4].Value?.ToString() ?? string.Empty,
                Pages = GetPageCount(printEvent),
                Status = "Completed",
                CompletedAt = printEvent.TimeCreated
            };

            var wasSent = await SendPrintEventAsync(
                printJob,
                stoppingToken);

            if (!wasSent)
            {
                break;
            }

            await SaveCheckpointAsync(recordId, stoppingToken);
            lastRecordId = recordId;
        }

        return lastRecordId;
    }

    private async Task SaveCheckpointAsync(
        long recordId,
        CancellationToken stoppingToken)
    {
        await _checkpointStore.SaveLastRecordIdAsync(
            recordId,
            stoppingToken);

        _logger.LogInformation(
            "Checkpoint atualizado para o evento {RecordId}",
            recordId);
    }

    private async Task<bool> SendPrintEventAsync(
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

            return true;
        }

        var error = await response.Content.ReadAsStringAsync(stoppingToken);

        _logger.LogError(
            "Falha ao enviar evento {SourceEventId}. HTTP {StatusCode}: {Error}",
            printEvent.SourceEventId,
            (int)response.StatusCode,
            error);

        return false;
    }

    private static int GetPageCount(EventRecord printEvent)
    {
        return int.TryParse(
            printEvent.Properties[7].Value?.ToString(),
            out var pages)
            ? pages
            : 1;
    }
}