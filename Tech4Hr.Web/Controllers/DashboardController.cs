using Microsoft.AspNetCore.Mvc;
using Tech4Hr.Web.Models;
using Tech4Hr.Web.Services;

namespace Tech4Hr.Web.Controllers;

public class DashboardController : Controller
{
    private readonly IFuncionarioService _funcionarioService;
    private readonly IUsuarioService _usuarioService;
    private readonly IPontoService _pontoService;
    private readonly TimeProvider _relogio;

    public DashboardController(
        IFuncionarioService funcionarioService,
        IUsuarioService usuarioService,
        IPontoService pontoService,
        TimeProvider relogio)
    {
        _funcionarioService = funcionarioService;
        _usuarioService = usuarioService;
        _pontoService = pontoService;
        _relogio = relogio;
    }

    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var token = HttpContext.Session.GetString("AuthToken");

        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            var funcionarios =
                await _funcionarioService.ListarAsync(
                    token,
                    cancellationToken);

            var nivelUsuario = HttpContext.Session.GetString("UsuarioNivel");
            var ehAdmin = string.Equals(
                nivelUsuario,
                "ADMIN",
                StringComparison.OrdinalIgnoreCase);

            var model = new DashboardViewModel
            {
                TotalFuncionarios = funcionarios.Count,
                FuncionariosAtivos = funcionarios.Count(f => f.Ativo),

                // Operacional agora é funcionário com nível OPERACIONAL.
                TotalOperacionais = funcionarios.Count(f => f.EhOperacional)
            };

            if (ehAdmin)
            {
                var usuarios =
                    await _usuarioService.ListarAsync(
                        token,
                        cancellationToken);

                model.TotalUsuarios = usuarios.Count;
                model.UsuariosAtivos = usuarios.Count(u => u.Ativo);
                model.TotalAdministradores = usuarios.Count(u => u.NivelUsuario == "ADMIN");
            }

            model.Graficos = await MontarGraficosAsync(
                token,
                funcionarios,
                cancellationToken);

            return View(model);
        }
        catch (HttpRequestException)
        {
            TempData["Erro"] =
                "Não foi possível carregar os dados do dashboard.";

            return View(new DashboardViewModel());
        }
    }

    // Os gráficos de ponto dependem de uma consulta a mais. Se ela falhar, o resumo
    // continua na tela e só os gráficos de ponto dão lugar a um aviso.
    private async Task<PainelDeGraficos> MontarGraficosAsync(
        string token,
        IReadOnlyList<FuncionarioApiResponse> funcionarios,
        CancellationToken cancellationToken)
    {
        var equipe = GraficosDoPainel.Equipe(funcionarios);

        try
        {
            var hoje = DiaDeBrasilia.Hoje(_relogio);
            var inicio = hoje.AddDays(-(GraficosDoPainel.DiasDoPeriodo - 1));

            var pontos = await _pontoService.ConsultarAsync(
                token,
                null,
                inicio.ToDateTime(TimeOnly.MinValue),
                hoje.ToDateTime(TimeOnly.MinValue),
                cancellationToken);

            return new PainelDeGraficos
            {
                Equipe = equipe,
                Hoje = GraficosDoPainel.Hoje(funcionarios, pontos, hoje),
                Dias = GraficosDoPainel.Dias(pontos, hoje)
            };
        }
        catch (HttpRequestException)
        {
            return new PainelDeGraficos
            {
                Equipe = equipe,
                Aviso = "Não foi possível carregar os registros de ponto para os gráficos."
            };
        }
    }
}
