using System.ComponentModel.DataAnnotations;

namespace Tech4Hr.PublicApi.Models;

public static class AbsenceTypes
{
    public const string Ferias = "FERIAS";
    public const string Falta = "FALTA";
    public const string Atestado = "ATESTADO";
    public const string Afastamento = "AFASTAMENTO";
    public static readonly IReadOnlySet<string> All = new HashSet<string> { Ferias, Falta, Atestado, Afastamento };
}

public sealed class WorkSchedule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public int DayOfWeek { get; set; }
    public int StartMinute { get; set; }
    public int EndMinute { get; set; }
    public int BreakMinutes { get; set; }
    [MaxLength(100)] public string TimeZoneId { get; set; } = "America/Sao_Paulo";
    public bool Active { get; set; } = true;
}

public sealed class AbsenceRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    [MaxLength(30)] public string Type { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    [MaxLength(500)] public string Reason { get; set; } = string.Empty;
    [MaxLength(20)] public string Status { get; set; } = "PENDENTE";
    [MaxLength(500)] public string? ManagerNote { get; set; }
    public Guid? ResolvedByUserId { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class Holiday
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateOnly Date { get; set; }
    [MaxLength(150)] public string Name { get; set; } = string.Empty;
    [MaxLength(2)] public string? StateCode { get; set; }
    [MaxLength(100)] public string? City { get; set; }
}

public sealed class AuditEvent
{
    public long Id { get; set; }
    public Guid? UserId { get; set; }
    [MaxLength(80)] public string Action { get; set; } = string.Empty;
    [MaxLength(80)] public string EntityType { get; set; } = string.Empty;
    [MaxLength(100)] public string? EntityId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class Attachment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid? PointCorrectionId { get; set; }
    public Guid? AbsenceRequestId { get; set; }
    [MaxLength(255)] public string OriginalName { get; set; } = string.Empty;
    [MaxLength(100)] public string StoredName { get; set; } = string.Empty;
    [MaxLength(100)] public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
