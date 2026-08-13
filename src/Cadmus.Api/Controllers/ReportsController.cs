using Cadmus.Api.Contracts.Reports;
using Cadmus.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cadmus.Api.Controllers;

[ApiController]
[Route("api/v1/reports")]
public sealed class ReportsController : ControllerBase
{
    private readonly CadmusDbContext _dbContext;

    public ReportsController(CadmusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<PrintSummaryResponse>> GetSummary(
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to)
    {
        var end = to ?? DateTimeOffset.UtcNow;
        var start = from ?? end.AddDays(-30);

        if (start > end)
        {
            return BadRequest(
                "O parâmetro 'from' não pode ser posterior a 'to'.");
        }

        var jobs = _dbContext.PrintJobs
            .AsNoTracking()
            .Where(job =>
                job.CompletedAt.HasValue &&
                job.CompletedAt >= start &&
                job.CompletedAt <= end);

        var response = new PrintSummaryResponse
        {
            TotalJobs = await jobs.CountAsync(),
            TotalPages = await jobs.SumAsync(job => (int?)job.Pages) ?? 0,
            CompletedJobs = await jobs.CountAsync(
                job => job.Status == "Completed"),
            DistinctUsers = await jobs
                .Select(job => job.UserName)
                .Distinct()
                .CountAsync(),
            DistinctPrinters = await jobs
                .Select(job => job.PrinterName)
                .Distinct()
                .CountAsync(),
            From = start,
            To = end
        };

        return Ok(response);
    }
    [HttpGet("by-user")]
public async Task<ActionResult<IEnumerable<PrintUsageByUserResponse>>> GetByUser(
    [FromQuery] DateTimeOffset? from,
    [FromQuery] DateTimeOffset? to)
{
    var end = to ?? DateTimeOffset.UtcNow;
    var start = from ?? end.AddDays(-30);

    if (start > end)
    {
        return BadRequest(
            "O parâmetro 'from' não pode ser posterior a 'to'.");
    }

    var report = await _dbContext.PrintJobs
        .AsNoTracking()
        .Where(job =>
            job.CompletedAt.HasValue &&
            job.CompletedAt >= start &&
            job.CompletedAt <= end &&
            job.Status == "Completed")
        .GroupBy(job => job.UserName)
        .Select(group => new PrintUsageByUserResponse
        {
            UserName = group.Key,
            TotalJobs = group.Count(),
            TotalPages = group.Sum(job => job.Pages)
        })
        .OrderByDescending(item => item.TotalPages)
        .ThenBy(item => item.UserName)
        .ToListAsync();

    return Ok(report);
}
[HttpGet("by-printer")]
public async Task<ActionResult<IEnumerable<PrintUsageByPrinterResponse>>> GetByPrinter(
    [FromQuery] DateTimeOffset? from,
    [FromQuery] DateTimeOffset? to)
{
    var end = to ?? DateTimeOffset.UtcNow;
    var start = from ?? end.AddDays(-30);

    if (start > end)
    {
        return BadRequest(
            "O parâmetro 'from' não pode ser posterior a 'to'.");
    }

    var report = await _dbContext.PrintJobs
        .AsNoTracking()
        .Where(job =>
            job.CompletedAt.HasValue &&
            job.CompletedAt >= start &&
            job.CompletedAt <= end &&
            job.Status == "Completed")
        .GroupBy(job => job.PrinterName)
        .Select(group => new PrintUsageByPrinterResponse
        {
            PrinterName = group.Key,
            TotalJobs = group.Count(),
            TotalPages = group.Sum(job => job.Pages)
        })
        .OrderByDescending(item => item.TotalPages)
        .ThenBy(item => item.PrinterName)
        .ToListAsync();

    return Ok(report);
}
}