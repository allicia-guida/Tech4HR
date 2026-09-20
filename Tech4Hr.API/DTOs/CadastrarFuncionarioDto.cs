using System.ComponentModel.DataAnnotations;

namespace Tech4Hr.API.DTOs;

public class CadastrarFuncionarioDto
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
    [RegularExpression(@"^\d{11}$",
        ErrorMessage = "O CPF deve conter exatamente 11 números.")]
    public string CPF { get; set; } = string.Empty;

    [Required(ErrorMessage = "A data de admissão é obrigatória.")]
    public DateTime? DataAdmissao { get; set; }
}