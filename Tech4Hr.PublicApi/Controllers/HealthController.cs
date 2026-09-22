using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tech4Hr.PublicApi.Data;

namespace Tech4Hr.PublicApi.Controllers;

[ApiController]
[Route("health")]
public sealed class HealthController(PublicApiDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var databaseAvailable = await db.Database.CanConnectAsync(cancellationToken);
        return databaseAvailable
            ? Ok(new { status = "healthy", timestampUtc = DateTimeOffset.UtcNow })
            : StatusCode(StatusCodes.Status503ServiceUnavailable, new { status = "unhealthy" });
    }
}
