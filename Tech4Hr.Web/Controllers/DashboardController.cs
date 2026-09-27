using Microsoft.AspNetCore.Mvc;
using Tech4Hr.Web.Models;
using Tech4Hr.Web.Services;

namespace Tech4Hr.Web.Controllers;

public class DashboardController : Controller
{
    private readonly IFuncionarioService _funcionarioService;
    private readonly IUsuarioService _usuarioService;

    public DashboardController(
        IFuncionarioService funcionarioService,
        IUsuarioService usuarioService)
    {
        _funcionarioService = funcionarioService;
        _usuarioService = usuarioService;
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

            var usuarios =
                await _usuarioService.ListarAsync(
                    token,
                    cancellationToken);

            var model = new DashboardViewModel
            {
                TotalFuncionarios = funcionarios.Count,
                FuncionariosAtivos = funcionarios.Count(f => f.Ativo),
                TotalUsuarios = usuarios.Count,
                UsuariosAtivos = usuarios.Count(u => u.Ativo),
                TotalAdministradores = usuarios.Count(u => u.NivelUsuario == "ADMIN"),
                TotalOperacionais = usuarios.Count(u => u.NivelUsuario == "OPERACIONAL")
            };

            return View(model);
        }
        catch (HttpRequestException)
        {
            TempData["Erro"] =
                "Não foi possível carregar os dados do dashboard.";

            return View(new DashboardViewModel());
        }
    }
}
