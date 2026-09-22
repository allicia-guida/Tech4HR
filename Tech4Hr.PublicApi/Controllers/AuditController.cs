using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tech4Hr.PublicApi.Data;
using Tech4Hr.PublicApi.Models;

namespace Tech4Hr.PublicApi.Controllers;

[ApiController]
[Authorize(Roles = Roles.Administrador)]
[Route("api/v1/audit")]
public sealed class AuditController(PublicApiDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(DateTime? fromUtc, DateTime? toUtc, int limit = 200, CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 500);
        var start = fromUtc ?? DateTime.UtcNow.AddDays(-30);
        var end = toUtc ?? DateTime.UtcNow;
        var items = await db.AuditEvents.AsNoTracking().Where(x => x.CreatedAtUtc >= start && x.CreatedAtUtc <= end)
            .OrderByDescending(x => x.CreatedAtUtc).Take(limit).ToListAsync(cancellationToken);
        return Ok(items);
    }
}
