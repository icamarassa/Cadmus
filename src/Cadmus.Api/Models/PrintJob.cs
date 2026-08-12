namespace Cadmus.Api.Models;

public sealed class PrintJob
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string? SourceEventId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string DocumentName { get; set; } = string.Empty;

    public string PrinterName { get; set; } = string.Empty;

    public string? ClientComputer { get; set; }

    public DateTimeOffset SubmittedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? CompletedAt { get; set; }

    public int Pages { get; set; }

    public string Status { get; set; } = "Pending";
}