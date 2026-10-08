using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Tech4Hr.Web.Models;

public sealed class UsuarioApiResponse
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

    [JsonPropertyName("ativo")]
    public bool Ativo { get; set; }

    public string NomeCompleto =>
        $"{Nome} {Sobrenome}".Trim();
}

public sealed class UsuarioCadastroInputModel
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O sobrenome é obrigatório.")]
    [StringLength(100)]
    public string Sobrenome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [MinLength(8, ErrorMessage = "A senha deve ter pelo menos 8 caracteres.")]
    public string Senha { get; set; } = string.Empty;

    [Required(ErrorMessage = "O nível do usuário é obrigatório.")]
    [RegularExpression("^ADMIN$",
        ErrorMessage = "Nível inválido. Operacionais são cadastrados como funcionários.")]
    public string NivelUsuario { get; set; } = string.Empty;
}

public sealed class UsuarioEdicaoInputModel
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O sobrenome é obrigatório.")]
    [StringLength(100)]
    public string Sobrenome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [RegularExpression("^(ADMIN|OPERACIONAL)?$", ErrorMessage = "Nível inválido.")]
    public string? NivelUsuario { get; set; }
}

public sealed class UsuariosIndexViewModel
{
    public IReadOnlyList<UsuarioApiResponse> Usuarios { get; set; } =
        Array.Empty<UsuarioApiResponse>();
}

public sealed class UsuarioFormViewModel
{
    public string Nome { get; set; } = string.Empty;
    public string Sobrenome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? NivelUsuario { get; set; }
    public string? Senha { get; set; }
}
