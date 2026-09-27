using System.Text.Json.Serialization;

namespace Tech4Hr.Web.Models;

public sealed class AuthLoginRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("senha")]
    public string Senha { get; set; } = string.Empty;
}

public sealed class AuthApiLoginResponse
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("tipo")]
    public string Tipo { get; set; } = string.Empty;

    [JsonPropertyName("expiraEm")]
    public DateTimeOffset? ExpiraEm { get; set; }

    [JsonPropertyName("usuario")]
    public AuthApiUsuarioResponse? Usuario { get; set; }
}

public sealed class AuthApiUsuarioResponse
{
    [JsonPropertyName("idUsuario")]
    public int IdUsuario { get; set; }

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("sobrenome")]
    public string Sobrenome { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("nivelUsuario")]
    public string NivelUsuario { get; set; } = string.Empty;
}

public sealed class AuthenticatedUser
{
    public int IdUsuario { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Sobrenome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NivelUsuario { get; set; } = string.Empty;
}

public sealed class AuthLoginResult
{
    public bool IsSuccess { get; set; }
    public string? Token { get; set; }
    public string? TokenType { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public AuthenticatedUser? User { get; set; }
    public string? ErrorMessage { get; set; }

    public static AuthLoginResult Success(
        string token,
        string tokenType,
        DateTimeOffset? expiresAt,
        AuthenticatedUser user)
    {
        return new AuthLoginResult
        {
            IsSuccess = true,
            Token = token,
            TokenType = tokenType,
            ExpiresAt = expiresAt,
            User = user,
            ErrorMessage = null
        };
    }

    public static AuthLoginResult Failure(string message)
    {
        return new AuthLoginResult
        {
            IsSuccess = false,
            ErrorMessage = message
        };
    }
}
