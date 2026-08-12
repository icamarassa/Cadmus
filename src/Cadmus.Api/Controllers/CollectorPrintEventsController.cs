using Cadmus.Api.Contracts;
using Cadmus.Api.Data;
using Cadmus.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cadmus.Api.Controllers;

[ApiController]
[Route("api/v1/collector/print-events")]
public sealed class CollectorPrintEventsController : ControllerBase
{
    private readonly CadmusDbContext _dbContext;

    public CollectorPrintEventsController(CadmusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<ActionResult<PrintJob>> Create(
        CollectorPrintEventRequest request)
    {
        var existingJob = await _dbContext.PrintJobs
            .AsNoTracking()
            .FirstOrDefaultAsync(job =>
                job.SourceEventId == request.SourceEventId);

        if (existingJob is not null)
        {
            return Ok(existingJob);
        }

        var printJob = new PrintJob
        {
            Id = Guid.NewGuid(),
            SourceEventId = request.SourceEventId,
            UserName = request.UserName,
            DocumentName = request.DocumentName,
            PrinterName = request.PrinterName,
            ClientComputer = request.ClientComputer,
            SubmittedAt = DateTimeOffset.UtcNow,
            CompletedAt = request.CompletedAt,
            Pages = request.Pages,
            Status = request.Status
        };

        _dbContext.PrintJobs.Add(printJob);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(
            actionName: nameof(PrintJobsController.GetById),
            controllerName: "PrintJobs",
            routeValues: new { id = printJob.Id },
            value: printJob);
    }
}