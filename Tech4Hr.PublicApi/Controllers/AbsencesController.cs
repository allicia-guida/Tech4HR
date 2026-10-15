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
[Route("api/v1/absences")]
public sealed class AbsencesController(PublicApiDbContext db, ICurrentUser currentUser, IAuditService audit) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateAbsenceRequest request, CancellationToken cancellationToken)
    {
        var type = request.Type.Trim().ToUpperInvariant();
        if (!AbsenceTypes.All.Contains(type)) return BadRequest(new { message = "Tipo de ausência inválido." });
        if (request.EndDate < request.StartDate || request.EndDate.DayNumber - request.StartDate.DayNumber > 366)
            return BadRequest(new { message = "Período de ausência inválido." });
        var overlap = await db.AbsenceRequests.AnyAsync(x => x.UserId == currentUser.UserId && x.Status != "REJEITADO" && x.StartDate <= request.EndDate && x.EndDate >= request.StartDate, cancellationToken);
        if (overlap) return Conflict(new { message = "Já existe uma solicitação nesse período." });
        var item = new AbsenceRequest
        {
            UserId = currentUser.UserId,
            Type = type,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Reason = request.Reason.Trim()
        };
        db.AbsenceRequests.Add(item);
        audit.Add(currentUser.UserId, "CREATE", nameof(AbsenceRequest), item.Id);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Mine), new { }, ToResponse(item));
    }

    [HttpGet("mine")]
    public async Task<IActionResult> Mine(CancellationToken cancellationToken)
    {
        var items = await db.AbsenceRequests.AsNoTracking().Where(x => x.UserId == currentUser.UserId)
            .OrderByDescending(x => x.CreatedAtUtc).ToListAsync(cancellationToken);
        return Ok(items.Select(ToResponse));
    }

    [HttpGet("pending")]
    [Authorize(Roles = Roles.Administrador + "," + Roles.Gestor)]
    public async Task<IActionResult> Pending(CancellationToken cancellationToken)
    {
        var items = await db.AbsenceRequests.AsNoTracking().Include(x => x.User).Where(x => x.Status == "PENDENTE")
            .OrderBy(x => x.CreatedAtUtc).Select(x => new { x.Id, x.UserId, userName = x.User.Name, x.Type, x.StartDate, x.EndDate, x.Reason, x.Status, x.CreatedAtUtc })
            .ToListAsync(cancellationToken);
        return Ok(items);
    }

    [HttpPatch("{id:guid}/resolve")]
    [Authorize(Roles = Roles.Administrador + "," + Roles.Gestor)]
    public async Task<IActionResult> Resolve(Guid id, ResolveAbsenceRequest request, CancellationToken cancellationToken)
    {
        var item = await db.AbsenceRequests.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (item is null) return NotFound();
        if (item.Status != "PENDENTE") return Conflict(new { message = "Solicitação já finalizada." });
        item.Status = request.Approved ? "APROVADO" : "REJEITADO";
        item.ManagerNote = request.ManagerNote?.Trim();
        item.ResolvedByUserId = currentUser.UserId;
        item.ResolvedAtUtc = DateTime.UtcNow;
        audit.Add(currentUser.UserId, item.Status, nameof(AbsenceRequest), item.Id);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToResponse(item));
    }

    private static object ToResponse(AbsenceRequest item) => new
    {
        item.Id,
        item.Type,
        item.StartDate,
        item.EndDate,
        item.Reason,
        item.Status,
        item.ManagerNote,
        item.ResolvedAtUtc,
        item.CreatedAtUtc
    };
}
