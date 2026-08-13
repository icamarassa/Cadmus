namespace Cadmus.Api.Contracts.Reports;

public sealed class PrintSummaryResponse
{
    public int TotalJobs { get; init; }

    public int TotalPages { get; init; }

    public int CompletedJobs { get; init; }

    public int DistinctUsers { get; init; }

    public int DistinctPrinters { get; init; }

    public DateTimeOffset From { get; init; }

    public DateTimeOffset To { get; init; }
}