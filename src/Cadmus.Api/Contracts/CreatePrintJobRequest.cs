using System.ComponentModel.DataAnnotations;

namespace Cadmus.Api.Contracts;

public sealed class CreatePrintJobRequest
{
    [Required]
    [StringLength(120)]
    public string UserName { get; init; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string DocumentName { get; init; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string PrinterName { get; init; } = string.Empty;

    [StringLength(120)]
    public string? ClientComputer { get; init; }

    [Range(1, 10_000)]
    public int Pages { get; init; }

    [Required]
    [RegularExpression("Pending|Completed|Cancelled|Failed")]
    public string Status { get; init; } = string.Empty;

    public DateTimeOffset? CompletedAt { get; init; }
}