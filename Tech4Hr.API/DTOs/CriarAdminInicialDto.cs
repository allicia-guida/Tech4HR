using System.ComponentModel.DataAnnotations;

namespace Tech4Hr.API.DTOs;

public class CriarAdminInicialDto
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
    [MinLength(12, ErrorMessage = "A senha deve ter pelo menos 12 caracteres.")]
    public string Senha { get; set; } = string.Empty;
}