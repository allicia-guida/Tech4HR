using System.ComponentModel.DataAnnotations;

namespace Tech4Hr.API.DTOs;

public class RegistrarPontoDto
{
    [Required(ErrorMessage = "O tipo de registro é obrigatório.")]
    [RegularExpression(
        "^(ENTRADA|SAIDA_ALMOCO|ENTRADA_ALMOCO|SAIDA)$",
        ErrorMessage = "Tipo de registro inválido.")]
    public string TipoRegistro { get; set; } = string.Empty;
}
