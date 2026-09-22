namespace Tech4Hr.PublicApi.Configuration;

public sealed class JwtSettings
{
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public int ExpirationMinutes { get; init; } = 60;
}

public sealed class PasswordResetSettings
{
    public int ExpirationMinutes { get; init; } = 30;
    public string AppUrl { get; init; } = "tech4hr://reset-password";
}

public sealed class SmtpSettings
{
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 587;
    public bool EnableSsl { get; init; } = true;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string From { get; init; } = string.Empty;
}
