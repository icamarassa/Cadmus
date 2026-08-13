namespace Cadmus.Api.Contracts.Reports;

public sealed class PrintUsageByPrinterResponse
{
    public string PrinterName { get; init; } = string.Empty;

    public int TotalJobs { get; init; }

    public int TotalPages { get; init; }
}