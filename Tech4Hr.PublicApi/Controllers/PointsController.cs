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
[Route("api/v1/points")]
public sealed class PointsController(PublicApiDbContext db, ICurrentUser currentUser, IAuditService audit) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Register(RegisterPointRequest request, CancellationToken cancellationToken)
    {
        var type = request.Type.Trim().ToUpperInvariant();
        if (!PointTypes.All.Contains(type))
        {
            return BadRequest(new { message = "Tipo de ponto inválido." });
        }
        if (request.Latitude is < -90 or > 90 || request.Longitude is < -180 or > 180)
        {
            return BadRequest(new { message = "Coordenadas inválidas." });
        }
        if ((request.Latitude is null) != (request.Longitude is null))
        {
            return BadRequest(new { message = "Latitude e longitude devem ser informadas juntas." });
        }
        var now = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            var existing = await db.TimeEntries.AsNoTracking().SingleOrDefaultAsync(x =>
                x.UserId == currentUser.UserId && x.IdempotencyKey == request.IdempotencyKey, cancellationToken);
            if (existing is not null)
            {
                return Ok(ToResponse(existing));
            }
        }
        var recentDuplicate = await db.TimeEntries.AnyAsync(x =>
            x.UserId == currentUser.UserId && x.Type == type && x.RecordedAtUtc >= now.AddSeconds(-30), cancellationToken);
        if (recentDuplicate)
        {
            return Conflict(new { message = "Registro duplicado. Aguarde antes de tentar novamente." });
        }
        var entry = new TimeEntry
        {
            UserId = currentUser.UserId,
            Type = type,
            RecordedAtUtc = now,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            AccuracyMeters = request.AccuracyMeters,
            DeviceReference = request.DeviceReference?.Trim(),
            IdempotencyKey = request.IdempotencyKey?.Trim()
        };
        db.TimeEntries.Add(entry);
        audit.Add(currentUser.UserId, "CREATE", nameof(TimeEntry), entry.Id);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = entry.Id }, ToResponse(entry));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var entry = await db.TimeEntries.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.UserId == currentUser.UserId, cancellationToken);
        return entry is null ? NotFound() : Ok(ToResponse(entry));
    }

    [HttpGet("history")]
    public async Task<IActionResult> History(DateTimeOffset? from, DateTimeOffset? to, string? type, CancellationToken cancellationToken)
    {
        var start = (from ?? DateTimeOffset.UtcNow.AddDays(-31)).UtcDateTime;
        var end = (to ?? DateTimeOffset.UtcNow).UtcDateTime;
        if (end < start || end - start > TimeSpan.FromDays(366))
        {
            return BadRequest(new { message = "Período inválido. O limite é de 366 dias." });
        }
        var query = db.TimeEntries.AsNoTracking().Where(x => x.UserId == currentUser.UserId && x.RecordedAtUtc >= start && x.RecordedAtUtc <= end);
        if (!string.IsNullOrWhiteSpace(type))
        {
            var normalizedType = type.Trim().ToUpperInvariant();
            query = query.Where(x => x.Type == normalizedType);
        }
        var entries = await query.OrderByDescending(x => x.RecordedAtUtc).ToListAsync(cancellationToken);
        return Ok(entries.Select(ToResponse));
    }

    private static object ToResponse(TimeEntry entry) => new
    {
        entry.Id,
        entry.Type,
        entry.RecordedAtUtc,
        location = entry.Latitude is null ? null : new { entry.Latitude, entry.Longitude, entry.AccuracyMeters },
        entry.DeviceReference,
        entry.IdempotencyKey
    };
}
