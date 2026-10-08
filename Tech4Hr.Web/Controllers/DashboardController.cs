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
