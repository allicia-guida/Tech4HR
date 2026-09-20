using System.ComponentModel.DataAnnotations;

namespace Tech4Hr.API.DTOs;

public class AlterarFuncionarioStatusDto
{
    [Required(ErrorMessage = "O status é obrigatório.")]
    public bool Ativo { get; set; }
}
