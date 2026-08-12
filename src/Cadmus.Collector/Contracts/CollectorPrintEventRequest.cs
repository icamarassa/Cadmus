namespace Cadmus.Collector.Contracts;

public sealed class CollectorPrintEventRequest
{
    public string SourceEventId { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string DocumentName { get; init; } = string.Empty;
    public string PrinterName { get; init; } = string.Empty;
    public string? ClientComputer { get; init; }
    public int Pages { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset? CompletedAt { get; init; }
}