using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tech4Hr.PublicApi.Data;
using Tech4Hr.PublicApi.Models;
using Tech4Hr.PublicApi.Services;

namespace Tech4Hr.PublicApi.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/summaries")]
public sealed class SummariesController(PublicApiDbContext db, ICurrentUser currentUser) : ControllerBase
{
    [HttpGet("mine")]
    public async Task<IActionResult> Mine(DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        if (to < from || to.DayNumber - from.DayNumber > 366)
            return BadRequest(new { message = "Período inválido. O limite é de 366 dias." });

        var schedules = await db.WorkSchedules.AsNoTracking().Where(x => x.UserId == currentUser.UserId && x.Active).ToListAsync(cancellationToken);
        var timeZoneId = schedules.Select(x => x.TimeZoneId).FirstOrDefault() ?? "America/Sao_Paulo";
        TimeZoneInfo timeZone;
        try
        {
            timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return Problem(statusCode: 500, title: "Fuso horário da jornada inválido.");
        }

        var startUtc = TimeZoneInfo.ConvertTimeToUtc(from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified), timeZone);
        var endUtc = TimeZoneInfo.ConvertTimeToUtc(to.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified), timeZone);
        var entries = await db.TimeEntries.AsNoTracking()
            .Where(x => x.UserId == currentUser.UserId && x.RecordedAtUtc >= startUtc && x.RecordedAtUtc < endUtc)
            .OrderBy(x => x.RecordedAtUtc).ToListAsync(cancellationToken);
        var holidays = await db.Holidays.AsNoTracking().Where(x => x.Date >= from && x.Date <= to).Select(x => x.Date).ToListAsync(cancellationToken);
        var absences = await db.AbsenceRequests.AsNoTracking()
            .Where(x => x.UserId == currentUser.UserId && x.Status == "APROVADO" && x.StartDate <= to && x.EndDate >= from)
            .ToListAsync(cancellationToken);

        var entriesByDate = entries.GroupBy(x => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(x.RecordedAtUtc, DateTimeKind.Utc), timeZone)))
            .ToDictionary(x => x.Key, x => x.OrderBy(e => e.RecordedAtUtc).ToList());
        var holidaySet = holidays.ToHashSet();
        var days = new List<object>();
        var expectedTotal = 0;
        var workedTotal = 0;
        var delayTotal = 0;
        var overtimeTotal = 0;

        for (var date = from; date <= to; date = date.AddDays(1))
        {
            var schedule = schedules.SingleOrDefault(x => x.DayOfWeek == (int)date.DayOfWeek);
            var excused = holidaySet.Contains(date) || absences.Any(x => x.StartDate <= date && x.EndDate >= date);
            var expected = schedule is null || excused ? 0 : schedule.EndMinute - schedule.StartMinute - schedule.BreakMinutes;
            entriesByDate.TryGetValue(date, out var dayEntries);
            dayEntries ??= new List<TimeEntry>();
            var entrada = dayEntries.FirstOrDefault(x => x.Type == PointTypes.Entrada);
            var saida = dayEntries.LastOrDefault(x => x.Type == PointTypes.Saida);
            var breakMinutes = CalculateBreakMinutes(dayEntries);
            var worked = entrada is null || saida is null || saida.RecordedAtUtc <= entrada.RecordedAtUtc
                ? 0
                : Math.Max(0, (int)Math.Round((saida.RecordedAtUtc - entrada.RecordedAtUtc).TotalMinutes) - breakMinutes);
            var arrivalMinute = entrada is null ? (int?)null : (int)TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(entrada.RecordedAtUtc, DateTimeKind.Utc), timeZone).TimeOfDay.TotalMinutes;
            var delay = schedule is null || arrivalMinute is null || excused ? 0 : Math.Max(0, arrivalMinute.Value - schedule.StartMinute);
            var balance = worked - expected;
            var overtime = Math.Max(0, balance);
            expectedTotal += expected;
            workedTotal += worked;
            delayTotal += delay;
            overtimeTotal += overtime;
            days.Add(new
            {
                date,
                expectedMinutes = expected,
                workedMinutes = worked,
                breakMinutes,
                delayMinutes = delay,
                overtimeMinutes = overtime,
                balanceMinutes = balance,
                incomplete = expected > 0 && (entrada is null || saida is null),
                excused,
                entries = dayEntries.Select(x => new { x.Id, x.Type, x.RecordedAtUtc })
            });
        }

        return Ok(new
        {
            from,
            to,
            timeZoneId,
            expectedMinutes = expectedTotal,
            workedMinutes = workedTotal,
            delayMinutes = delayTotal,
            overtimeMinutes = overtimeTotal,
            balanceMinutes = workedTotal - expectedTotal,
            days
        });
    }

    private static int CalculateBreakMinutes(IReadOnlyList<TimeEntry> entries)
    {
        DateTime? start = null;
        var total = 0;
        foreach (var entry in entries)
        {
            if (entry.Type == PointTypes.InicioIntervalo)
            {
                start = entry.RecordedAtUtc;
            }
            else if (entry.Type == PointTypes.FimIntervalo && start is not null && entry.RecordedAtUtc > start)
            {
                total += (int)Math.Round((entry.RecordedAtUtc - start.Value).TotalMinutes);
                start = null;
            }
        }
        return Math.Max(0, total);
    }
}
