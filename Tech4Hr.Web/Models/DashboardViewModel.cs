namespace Tech4Hr.Web.Models;

public class DashboardViewModel
{
    public int TotalFuncionarios { get; set; }
    public int FuncionariosAtivos { get; set; }
    public int TotalUsuarios { get; set; }
    public int UsuariosAtivos { get; set; }
    public int TotalAdministradores { get; set; }
    public int TotalOperacionais { get; set; }

    /// <summary>Gráficos do painel. Nulo quando nem a lista de funcionários carregou.</summary>
    public PainelDeGraficos? Graficos { get; set; }
}
