using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Tech4Hr.Web.Models;

public sealed class FuncionarioApiResponse
{
    [JsonPropertyName("idFuncionario")]
    public int IdFuncionario { get; set; }

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("sobrenome")]
    public string Sobrenome { get; set; } = string.Empty;

    [JsonPropertyName("emailCorporativo")]
    public string EmailCorporativo { get; set; } = string.Empty;

    [JsonPropertyName("cpf")]
    public string CPF { get; set; } = string.Empty;

    [JsonPropertyName("dataAdmissao")]
    public DateTime DataAdmissao { get; set; }

    [JsonPropertyName("ativo")]
    public bool Ativo { get; set; }

    public string NomeCompleto =>
        $"{Nome} {Sobrenome}".Trim();
}

public sealed class FuncionarioCadastroInputModel
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O sobrenome é obrigatório.")]
    [StringLength(100)]
    public string Sobrenome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail corporativo é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(255)]
    public string EmailCorporativo { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [MinLength(8, ErrorMessage = "A senha deve ter pelo menos 8 caracteres.")]
    public string Senha { get; set; } = string.Empty;

    [Required(ErrorMessage = "O CPF é obrigatório.")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "O CPF deve conter exatamente 11 números.")]
    public string CPF { get; set; } = string.Empty;

    [Required(ErrorMessage = "A data de admissão é obrigatória.")]
    public DateTime? DataAdmissao { get; set; }
}

public sealed class FuncionarioEdicaoInputModel
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O sobrenome é obrigatório.")]
    [StringLength(100)]
    public string Sobrenome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail corporativo é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(255)]
    public string EmailCorporativo { get; set; } = string.Empty;

    [Required(ErrorMessage = "O CPF é obrigatório.")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "O CPF deve conter exatamente 11 números.")]
    public string CPF { get; set; } = string.Empty;

    [Required(ErrorMessage = "A data de admissão é obrigatória.")]
    public DateTime? DataAdmissao { get; set; }
}