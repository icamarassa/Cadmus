using Microsoft.AspNetCore.Mvc;

namespace Cadmus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "online",
            service = "Cadmus API",
            checkedAt = DateTimeOffset.UtcNow
        });
    }
}