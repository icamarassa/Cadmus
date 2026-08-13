namespace Cadmus.Api.Contracts.Reports;

public sealed class PrintUsageByUserResponse
{
    public string UserName { get; init; } = string.Empty;

    public int TotalJobs { get; init; }

    public int TotalPages { get; init; }
}