using System.ComponentModel.DataAnnotations;

namespace Tech4Hr.PublicApi.Models;

public static class Roles
{
    public const string Administrador = "ADMIN";
    public const string Gestor = "GESTOR";
    public const string Funcionario = "FUNCIONARIO";
}

public static class PointTypes
{
    public const string Entrada = "ENTRADA";
    public const string InicioIntervalo = "INICIO_INTERVALO";
    public const string FimIntervalo = "FIM_INTERVALO";
    public const string Saida = "SAIDA";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        Entrada,
        InicioIntervalo,
        FimIntervalo,
        Saida
    };
}

public sealed class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [MaxLength(150)] public string Name { get; set; } = string.Empty;
    [MaxLength(254)] public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    [MaxLength(20)] public string Role { get; set; } = Roles.Funcionario;
    public bool Active { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}

public sealed class TimeEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    [MaxLength(30)] public string Type { get; set; } = string.Empty;
    public DateTime RecordedAtUtc { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? AccuracyMeters { get; set; }
    [MaxLength(100)] public string? DeviceReference { get; set; }
    [MaxLength(100)] public string? IdempotencyKey { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class PointCorrection
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid? TimeEntryId { get; set; }
    public TimeEntry? TimeEntry { get; set; }
    public DateTime RequestedTimeUtc { get; set; }
    [MaxLength(500)] public string Reason { get; set; } = string.Empty;
    [MaxLength(20)] public string Status { get; set; } = "PENDENTE";
    [MaxLength(500)] public string? ManagerNote { get; set; }
    public Guid? ResolvedByUserId { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class PasswordResetToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    [MaxLength(64)] public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? UsedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
