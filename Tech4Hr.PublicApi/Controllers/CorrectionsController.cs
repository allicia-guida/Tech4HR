using System.Security.Claims;
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
[Route("api/v1/corrections")]
public sealed class CorrectionsController(PublicApiDbContext db, ICurrentUser currentUser, IAuditService audit) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateCorrectionRequest request, CancellationToken cancellationToken)
    {
        if (request.TimeEntryId is not null && !await db.TimeEntries.AnyAsync(x => x.Id == request.TimeEntryId && x.UserId == currentUser.UserId, cancellationToken))
        {
            return BadRequest(new { message = "Registro de ponto não encontrado." });
        }
        var correction = new PointCorrection
        {
            UserId = currentUser.UserId,
            TimeEntryId = request.TimeEntryId,
            RequestedTimeUtc = request.RequestedTime.UtcDateTime,
            Reason = request.Reason.Trim()
        };
        db.PointCorrections.Add(correction);
        audit.Add(currentUser.UserId, "CREATE", nameof(PointCorrection), correction.Id);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetMine), new { }, ToResponse(correction));
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var items = await db.PointCorrections.AsNoTracking().Where(x => x.UserId == currentUser.UserId)
            .OrderByDescending(x => x.CreatedAtUtc).ToListAsync(cancellationToken);
        return Ok(items.Select(ToResponse));
    }

    [HttpGet("pending")]
    [Authorize(Roles = Roles.Administrador + "," + Roles.Gestor)]
    public async Task<IActionResult> Pending(CancellationToken cancellationToken)
    {
        var items = await db.PointCorrections.AsNoTracking().Include(x => x.User).Where(x => x.Status == "PENDENTE")
            .OrderBy(x => x.CreatedAtUtc).Select(x => new
            {
                x.Id,
                x.UserId,
                userName = x.User.Name,
                x.TimeEntryId,
                x.RequestedTimeUtc,
                x.Reason,
                x.Status,
                x.CreatedAtUtc
            }).ToListAsync(cancellationToken);
        return Ok(items);
    }

    [HttpPatch("{id:guid}/resolve")]
    [Authorize(Roles = Roles.Administrador + "," + Roles.Gestor)]
    public async Task<IActionResult> Resolve(Guid id, ResolveCorrectionRequest request, CancellationToken cancellationToken)
    {
        var correction = await db.PointCorrections.Include(x => x.TimeEntry).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (correction is null) return NotFound();
        if (correction.Status != "PENDENTE") return Conflict(new { message = "Solicitação já finalizada." });
        correction.Status = request.Approved ? "APROVADO" : "REJEITADO";
        correction.ManagerNote = request.ManagerNote?.Trim();
        correction.ResolvedByUserId = currentUser.UserId;
        correction.ResolvedAtUtc = DateTime.UtcNow;
        if (request.Approved && correction.TimeEntry is not null)
        {
            correction.TimeEntry.RecordedAtUtc = correction.RequestedTimeUtc;
        }
        audit.Add(currentUser.UserId, correction.Status, nameof(PointCorrection), correction.Id);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToResponse(correction));
    }

    private static object ToResponse(PointCorrection item) => new
    {
        item.Id,
        item.TimeEntryId,
        item.RequestedTimeUtc,
        item.Reason,
        item.Status,
        item.ManagerNote,
        item.ResolvedAtUtc,
        item.CreatedAtUtc
    };
}
