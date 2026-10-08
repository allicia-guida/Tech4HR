using System.Text.Json.Serialization;

namespace Tech4Hr.Web.Models;

public sealed class AuthLoginRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("senha")]
    public string Senha { get; set; } = string.Empty;
}

public sealed class FuncionarioAuthLoginRequest
{
    [JsonPropertyName("emailCorporativo")]
    public string EmailCorporativo { get; set; } = string.Empty;

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

public sealed class FuncionarioAuthApiLoginResponse
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("tipo")]
    public string Tipo { get; set; } = string.Empty;

    [JsonPropertyName("expiraEm")]
    public DateTimeOffset? ExpiraEm { get; set; }

    [JsonPropertyName("funcionario")]
    public FuncionarioAuthApiFuncionarioResponse? Funcionario { get; set; }
}

public sealed class FuncionarioAuthApiFuncionarioResponse
{
    [JsonPropertyName("idFuncionario")]
    public int IdFuncionario { get; set; }

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("sobrenome")]
    public string Sobrenome { get; set; } = string.Empty;

    [JsonPropertyName("emailCorporativo")]
    public string EmailCorporativo { get; set; } = string.Empty;

    [JsonPropertyName("ativo")]
    public bool Ativo { get; set; }

    [JsonPropertyName("nivelAcesso")]
    public string NivelAcesso { get; set; } = "FUNCIONARIO";
}

public sealed class AuthenticatedUser
{
    public int IdUsuario { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Sobrenome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NivelUsuario { get; set; } = string.Empty;
}

public sealed class FuncionarioAuthenticatedUser
{
    public int IdFuncionario { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Sobrenome { get; set; } = string.Empty;
    public string EmailCorporativo { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public string NivelAcesso { get; set; } = "FUNCIONARIO";

    public bool EhOperacional =>
        string.Equals(NivelAcesso, "OPERACIONAL", StringComparison.OrdinalIgnoreCase);
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

public sealed class FuncionarioAuthLoginResult
{
    public bool IsSuccess { get; set; }
    public string? Token { get; set; }
    public string? TokenType { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public FuncionarioAuthenticatedUser? User { get; set; }
    public string? ErrorMessage { get; set; }

    public static FuncionarioAuthLoginResult Success(
        string token,
        string tokenType,
        DateTimeOffset? expiresAt,
        FuncionarioAuthenticatedUser user)
    {
        return new FuncionarioAuthLoginResult
        {
            IsSuccess = true,
            Token = token,
            TokenType = tokenType,
            ExpiresAt = expiresAt,
            User = user,
            ErrorMessage = null
        };
    }

    public static FuncionarioAuthLoginResult Failure(string message)
    {
        return new FuncionarioAuthLoginResult
        {
            IsSuccess = false,
            ErrorMessage = message
        };
    }
}
