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
[Route("api/v1/schedules")]
public sealed class SchedulesController(PublicApiDbContext db, ICurrentUser currentUser, IAuditService audit) : ControllerBase
{
    [HttpGet("mine")]
    public async Task<IActionResult> Mine(CancellationToken cancellationToken)
    {
        var items = await db.WorkSchedules.AsNoTracking().Where(x => x.UserId == currentUser.UserId)
            .OrderBy(x => x.DayOfWeek).ToListAsync(cancellationToken);
        return Ok(items.Select(ToResponse));
    }

    [HttpPut]
    [Authorize(Roles = Roles.Administrador + "," + Roles.Gestor)]
    public async Task<IActionResult> Upsert(UpsertScheduleRequest request, CancellationToken cancellationToken)
    {
        if (request.EndMinute <= request.StartMinute || request.BreakMinutes >= request.EndMinute - request.StartMinute)
        {
            return BadRequest(new { message = "Horários da jornada inválidos." });
        }
        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(request.TimeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return BadRequest(new { message = "Fuso horário inválido." });
        }
        if (!await db.Users.AnyAsync(x => x.Id == request.UserId && x.Active, cancellationToken)) return NotFound();
        var item = await db.WorkSchedules.SingleOrDefaultAsync(x => x.UserId == request.UserId && x.DayOfWeek == request.DayOfWeek, cancellationToken);
        if (item is null)
        {
            item = new WorkSchedule { UserId = request.UserId, DayOfWeek = request.DayOfWeek };
            db.WorkSchedules.Add(item);
        }
        item.StartMinute = request.StartMinute;
        item.EndMinute = request.EndMinute;
        item.BreakMinutes = request.BreakMinutes;
        item.TimeZoneId = request.TimeZoneId;
        item.Active = request.Active;
        audit.Add(currentUser.UserId, "UPSERT", nameof(WorkSchedule), item.Id);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToResponse(item));
    }

    private static object ToResponse(WorkSchedule item) => new
    {
        item.Id,
        item.UserId,
        item.DayOfWeek,
        item.StartMinute,
        item.EndMinute,
        item.BreakMinutes,
        item.TimeZoneId,
        item.Active
    };
}
