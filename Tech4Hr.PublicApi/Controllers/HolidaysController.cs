using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tech4Hr.PublicApi.Contracts;
using Tech4Hr.PublicApi.Data;
using Tech4Hr.PublicApi.Models;
using Tech4Hr.PublicApi.Services;

namespace Tech4Hr.PublicApi.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/holidays")]
public sealed class HolidaysController(PublicApiDbContext db, ICurrentUser currentUser, IAuditService audit) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(DateOnly? from, DateOnly? to, CancellationToken cancellationToken)
    {
        var start = from ?? new DateOnly(DateTime.UtcNow.Year, 1, 1);
        var end = to ?? new DateOnly(DateTime.UtcNow.Year, 12, 31);
        var items = await db.Holidays.AsNoTracking().Where(x => x.Date >= start && x.Date <= end).OrderBy(x => x.Date).ToListAsync(cancellationToken);
        return Ok(items);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Create(CreateHolidayRequest request, CancellationToken cancellationToken)
    {
        var item = new Holiday
        {
            Date = request.Date,
            Name = request.Name.Trim(),
            StateCode = request.StateCode?.Trim().ToUpperInvariant(),
            City = request.City?.Trim()
        };
        db.Holidays.Add(item);
        audit.Add(currentUser.UserId, "CREATE", nameof(Holiday), item.Id);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "Feriado já cadastrado." });
        }
        return Created($"/api/v1/holidays/{item.Id}", item);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var item = await db.Holidays.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (item is null) return NotFound();
        db.Holidays.Remove(item);
        audit.Add(currentUser.UserId, "DELETE", nameof(Holiday), id);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
