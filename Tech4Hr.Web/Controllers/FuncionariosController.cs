using Microsoft.AspNetCore.Mvc;
using Tech4Hr.Web.Models;
using Tech4Hr.Web.Services;

namespace Tech4Hr.Web.Controllers;

public class FuncionariosController : Controller
{
    private readonly IFuncionarioService _funcionarioService;

    public FuncionariosController(
        IFuncionarioService funcionarioService)
    {
        _funcionarioService = funcionarioService;
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

            return View(funcionarios);
        }
        catch (HttpRequestException)
        {
            TempData["Erro"] =
                "Não foi possível carregar os funcionários.";

            return View(Array.Empty<FuncionarioApiResponse>());
        }
    }

    public async Task<IActionResult> Detalhes(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        var token = HttpContext.Session.GetString("AuthToken");

        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            var funcionario =
                await _funcionarioService.BuscarPorIdAsync(
                    id,
                    token,
                    cancellationToken);

            if (funcionario is null)
            {
                return NotFound();
            }

            return View(funcionario);
        }
        catch (HttpRequestException)
        {
            TempData["Erro"] =
                "Não foi possível carregar os dados do funcionário.";

            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public IActionResult Cadastrar()
    {
        var token = HttpContext.Session.GetString("AuthToken");

        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        return View(new FuncionarioCadastroInputModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cadastrar(
        FuncionarioCadastroInputModel model,
        CancellationToken cancellationToken)
    {
        var token = HttpContext.Session.GetString("AuthToken");

        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _funcionarioService.CriarAsync(
                model,
                token,
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
        var token = HttpContext.Session.GetString("AuthToken");

        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        if (id <= 0)
        {
            return NotFound();
        }

        var funcionario =
            await _funcionarioService.BuscarPorIdAsync(
                id,
                token,
                cancellationToken);

        if (funcionario is null)
        {
            return NotFound();
        }

        var model = new FuncionarioEdicaoInputModel
        {
            Nome = funcionario.Nome,
            Sobrenome = funcionario.Sobrenome,
            EmailCorporativo = funcionario.EmailCorporativo,
            CPF = funcionario.CPF,
            DataAdmissao = funcionario.DataAdmissao
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(
        int id,
        FuncionarioEdicaoInputModel model,
        CancellationToken cancellationToken)
    {
        var token = HttpContext.Session.GetString("AuthToken");

        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        if (id <= 0)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var funcionario =
                await _funcionarioService.AtualizarAsync(
                    id,
                    model,
                    token,
                    cancellationToken);

            if (funcionario is null)
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
            var funcionario =
                await _funcionarioService.AlterarStatusAsync(
                    id,
                    ativo,
                    token!,
                    cancellationToken);

            if (funcionario is null)
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