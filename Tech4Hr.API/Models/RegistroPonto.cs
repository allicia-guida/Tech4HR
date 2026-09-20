namespace Tech4Hr.API.Models;

public class RegistroPonto
{
    public long IdRegistroPonto { get; set; }

    public int IdFuncionario { get; set; }
    public int IdPonto { get; set; }

    public string TipoRegistro { get; set; } = string.Empty;

    public DateTimeOffset DataHora { get; set; }

    public DateTimeOffset DataHoraRecebimento { get; set; }

    public Funcionario Funcionario { get; set; } = null!;

    public Ponto Ponto { get; set; } = null!;
}
