using Microsoft.AspNetCore.Mvc;
using Tech4Hr.Web.Models;
using Tech4Hr.Web.Services;

namespace Tech4Hr.Web.Controllers;

public class PontosController : Controller
{
    private readonly IPontoService _pontoService;
    private readonly IFuncionarioService _funcionarioService;

    public PontosController(
        IPontoService pontoService,
        IFuncionarioService funcionarioService)
    {
        _pontoService = pontoService;
        _funcionarioService = funcionarioService;
    }

    private bool UsuarioEhAdminOuOperacional()
    {
        var nivel = HttpContext.Session.GetString("UsuarioNivel");
        return string.Equals(nivel, "ADMIN", StringComparison.OrdinalIgnoreCase)
            || string.Equals(nivel, "OPERACIONAL", StringComparison.OrdinalIgnoreCase);
    }

    private bool UsuarioLogado()
    {
        return !string.IsNullOrWhiteSpace(HttpContext.Session.GetString("AuthToken"));
    }

    public async Task<IActionResult> Index(
        int? idFuncionario,
        DateTime? dataInicio,
        DateTime? dataFim,
        CancellationToken cancellationToken)
    {
        if (!UsuarioLogado())
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!UsuarioEhAdminOuOperacional())
        {
            TempData["Erro"] = "Acesso restrito a administradores e operacionais.";
            return RedirectToAction("Index", "Dashboard");
        }

        var token = HttpContext.Session.GetString("AuthToken");

        try
        {
            var funcionarios =
                await _funcionarioService.ListarAsync(
                    token!,
                    cancellationToken);

            var registros =
                await _pontoService.ConsultarAsync(
                    token!,
                    idFuncionario,
                    dataInicio,
                    dataFim,
                    cancellationToken);

            var model = new PontosConsultaViewModel
            {
                IdFuncionario = idFuncionario,
                DataInicio = dataInicio,
                DataFim = dataFim,
                Funcionarios = funcionarios,
                Registros = registros
                    .OrderByDescending(p => p.DataPonto)
                    .ToList()
            };

            return View(model);
        }
        catch (HttpRequestException)
        {
            TempData["Erro"] = "Não foi possível carregar os registros de ponto.";

            return View(new PontosConsultaViewModel
            {
                IdFuncionario = idFuncionario,
                DataInicio = dataInicio,
                DataFim = dataFim,
                Funcionarios = Array.Empty<FuncionarioApiResponse>()
            });
        }
    }
}

