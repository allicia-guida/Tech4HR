namespace Tech4Hr.API.Models;

public class Funcionario
{
    public int IdFuncionario { get; set; }

    public string Nome { get; set; } = string.Empty;
    public string Sobrenome { get; set; } = string.Empty;
    public string EmailCorporativo { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;

    public DateTime DataAdmissao { get; set; }

    public bool Ativo { get; set; } = true;

    public string CPF { get; set; } = string.Empty;

    public ICollection<Ponto> Pontos { get; set; }
        = new List<Ponto>();

    public ICollection<RegistroPonto> RegistrosPonto { get; set; }
        = new List<RegistroPonto>();
}
