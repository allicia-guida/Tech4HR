using System.ComponentModel.DataAnnotations;

namespace Tech4Hr.PublicApi.Contracts;

public sealed record LoginRequest(
    [Required, EmailAddress, MaxLength(254)] string Email,
    [Required, MinLength(8), MaxLength(128)] string Password);

public sealed record ForgotPasswordRequest(
    [Required, EmailAddress, MaxLength(254)] string Email);

public sealed record ResetPasswordRequest(
    [Required] string Token,
    [Required, MinLength(12), MaxLength(128), RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^A-Za-z0-9]).+$")] string NewPassword);

public sealed record CreateUserRequest(
    [Required, MaxLength(150)] string Name,
    [Required, EmailAddress, MaxLength(254)] string Email,
    [Required, MinLength(12), MaxLength(128), RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^A-Za-z0-9]).+$")] string Password,
    [Required, MaxLength(20)] string Role);

public sealed record RegisterPointRequest(
    [Required, MaxLength(30)] string Type,
    double? Latitude,
    double? Longitude,
    [Range(0, 10000)] double? AccuracyMeters,
    [MaxLength(100)] string? DeviceReference,
    [MaxLength(100)] string? IdempotencyKey);

public sealed record CreateCorrectionRequest(
    Guid? TimeEntryId,
    DateTimeOffset RequestedTime,
    [Required, MinLength(10), MaxLength(500)] string Reason);

public sealed record ResolveCorrectionRequest(
    bool Approved,
    [MaxLength(500)] string? ManagerNote);
