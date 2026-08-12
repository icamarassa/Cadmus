using Cadmus.Api.Data;
using Cadmus.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cadmus.Api.Controllers;

[ApiController]
[Route("api/print-jobs")]
public sealed class PrintJobsController : ControllerBase
{
    private readonly CadmusDbContext _dbContext;

    public PrintJobsController(CadmusDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PrintJob>>> GetAll()
    {
        var printJobs = await _dbContext.PrintJobs
            .OrderByDescending(job => job.SubmittedAt)
            .ToListAsync();

        return Ok(printJobs);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PrintJob>> GetById(Guid id)
    {
        var printJob = await _dbContext.PrintJobs.FindAsync(id);

        return printJob is null ? NotFound() : Ok(printJob);
    }

    [HttpPost]
    public async Task<ActionResult<PrintJob>> Create(PrintJob printJob)
    {
        printJob.Id = Guid.NewGuid();
        printJob.SubmittedAt = DateTimeOffset.UtcNow;

        _dbContext.PrintJobs.Add(printJob);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = printJob.Id }, printJob);
    }
}