using System.ComponentModel.DataAnnotations;

namespace Tech4Hr.API.DTOs;

public class LoginFuncionarioDto
{
    [Required(ErrorMessage = "O e-mail corporativo é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    public string EmailCorporativo { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    public string Senha { get; set; } = string.Empty;
}
