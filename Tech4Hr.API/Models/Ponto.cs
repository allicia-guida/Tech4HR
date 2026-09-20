namespace Tech4Hr.API.Models;

public class Ponto
{
    public int IdPonto { get; set; }
    public int IdFuncionario { get; set; }

    public DateTime DataPonto { get; set; }

    public DateTime? Entrada { get; set; }
    public DateTime? SaidaAlmoco { get; set; }
    public DateTime? EntradaAlmoco { get; set; }
    public DateTime? Saida { get; set; }

    public Funcionario Funcionario { get; set; } = null!;

    public ICollection<RegistroPonto> RegistrosPonto { get; set; }
        = new List<RegistroPonto>();
}
