using Microsoft.AspNetCore.Mvc;
using Tech4Hr.Web.Models;
using Tech4Hr.Web.Services;

namespace Tech4Hr.Web.Controllers;

public class UsuariosController : Controller
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    private bool UsuarioEhAdmin()
    {
        var nivel = HttpContext.Session.GetString("UsuarioNivel");
        return string.Equals(nivel, "ADMIN", StringComparison.OrdinalIgnoreCase);
    }

    private bool UsuarioLogado()
    {
        return !string.IsNullOrWhiteSpace(HttpContext.Session.GetString("AuthToken"));
    }

    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        if (!UsuarioLogado())
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!UsuarioEhAdmin())
        {
            TempData["Erro"] = "Acesso restrito a administradores.";
            return RedirectToAction("Index", "Dashboard");
        }

        var token = HttpContext.Session.GetString("AuthToken");

        try
        {
            var usuarios =
                await _usuarioService.ListarAsync(
                    token!,
                    cancellationToken);

            return View(usuarios);
        }
        catch (HttpRequestException)
        {
            TempData["Erro"] =
                "Não foi possível carregar os usuários.";

            return View(Array.Empty<UsuarioApiResponse>());
        }
    }

    [HttpGet]
    public IActionResult Cadastrar()
    {
        if (!UsuarioLogado())
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!UsuarioEhAdmin())
        {
            TempData["Erro"] = "Acesso restrito a administradores.";
            return RedirectToAction("Index", "Dashboard");
        }

        return View(new UsuarioCadastroInputModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cadastrar(
        UsuarioCadastroInputModel model,
        CancellationToken cancellationToken)
    {
        if (!UsuarioLogado())
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!UsuarioEhAdmin())
        {
            TempData["Erro"] = "Acesso restrito a administradores.";
            return RedirectToAction("Index", "Dashboard");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var token = HttpContext.Session.GetString("AuthToken");

        try
        {
            await _usuarioService.CriarAsync(
                model,
                token!,
                cancellationToken);

            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Editar(
        int id,
        CancellationToken cancellationToken)
    {
        if (!UsuarioLogado())
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!UsuarioEhAdmin())
        {
            TempData["Erro"] = "Acesso restrito a administradores.";
            return RedirectToAction("Index", "Dashboard");
        }

        if (id <= 0)
        {
            return NotFound();
        }

        var token = HttpContext.Session.GetString("AuthToken");

        var usuario =
            await _usuarioService.BuscarPorIdAsync(
                id,
                token!,
                cancellationToken);

        if (usuario is null)
        {
            return NotFound();
        }

        var model = new UsuarioEdicaoInputModel
        {
            Nome = usuario.Nome,
            Sobrenome = usuario.Sobrenome,
            Email = usuario.Email,
            NivelUsuario = usuario.NivelUsuario
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(
        int id,
        UsuarioEdicaoInputModel model,
        CancellationToken cancellationToken)
    {
        if (!UsuarioLogado())
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!UsuarioEhAdmin())
        {
            TempData["Erro"] = "Acesso restrito a administradores.";
            return RedirectToAction("Index", "Dashboard");
        }

        if (id <= 0)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var token = HttpContext.Session.GetString("AuthToken");

        try
        {
            var usuario =
                await _usuarioService.AtualizarAsync(
                    id,
                    model,
                    token!,
                    cancellationToken);

            if (usuario is null)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarStatus(
        int id,
        bool ativo,
        CancellationToken cancellationToken)
    {
        if (!UsuarioLogado())
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!UsuarioEhAdmin())
        {
            TempData["Erro"] = "Acesso restrito a administradores.";
            return RedirectToAction("Index", "Dashboard");
        }

        if (id <= 0)
        {
            return NotFound();
        }

        var token = HttpContext.Session.GetString("AuthToken");

        try
        {
            var usuario =
                await _usuarioService.AlterarStatusAsync(
                    id,
                    ativo,
                    token!,
                    cancellationToken);

            if (usuario is null)
            {
                return NotFound();
            }
        }
        catch (HttpRequestException ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
