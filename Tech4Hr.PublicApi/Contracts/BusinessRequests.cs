using System.ComponentModel.DataAnnotations;

namespace Tech4Hr.PublicApi.Contracts;

public sealed record UpsertScheduleRequest(
    Guid UserId,
    [Range(0, 6)] int DayOfWeek,
    [Range(0, 1439)] int StartMinute,
    [Range(1, 1440)] int EndMinute,
    [Range(0, 600)] int BreakMinutes,
    [Required, MaxLength(100)] string TimeZoneId,
    bool Active = true);

public sealed record CreateAbsenceRequest(
    [Required, MaxLength(30)] string Type,
    DateOnly StartDate,
    DateOnly EndDate,
    [Required, MinLength(5), MaxLength(500)] string Reason);

public sealed record ResolveAbsenceRequest(
    bool Approved,
    [MaxLength(500)] string? ManagerNote);

public sealed record CreateHolidayRequest(
    DateOnly Date,
    [Required, MaxLength(150)] string Name,
    [StringLength(2, MinimumLength = 2)] string? StateCode,
    [MaxLength(100)] string? City);
