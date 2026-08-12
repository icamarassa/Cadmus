using Cadmus.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Cadmus.Api.Controllers;

[ApiController]
[Route("api/print-jobs")]
public sealed class PrintJobsController : ControllerBase
{
    private static readonly List<PrintJob> Jobs = [];

    [HttpGet]
    public ActionResult<IEnumerable<PrintJob>> GetAll()
    {
        return Ok(Jobs);
    }

    [HttpGet("{id:guid}")]
    public ActionResult<PrintJob> GetById(Guid id)
    {
        var printJob = Jobs.FirstOrDefault(job => job.Id == id);

        return printJob is null ? NotFound() : Ok(printJob);
    }

    [HttpPost]
    public ActionResult<PrintJob> Create(PrintJob printJob)
    {
        printJob.Id = Guid.NewGuid();
        printJob.SubmittedAt = DateTimeOffset.UtcNow;

        Jobs.Add(printJob);

        return CreatedAtAction(nameof(GetById), new { id = printJob.Id }, printJob);
    }
}