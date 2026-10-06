using Microsoft.AspNetCore.Mvc;
using Tech4Hr.Web.Models;
using Tech4Hr.Web.Services;

namespace Tech4Hr.Web.Controllers;

public class FuncionariosController : Controller
{
    private readonly IFuncionarioService _funcionarioService;
    private readonly IPontoService _pontoService;

    public FuncionariosController(
        IFuncionarioService funcionarioService,
        IPontoService pontoService)
    {
        _funcionarioService = funcionarioService;
        _pontoService = pontoService;
    }

    private bool UsuarioEhAdminOuOperacional()
    {
        var nivel = HttpContext.Session.GetString("UsuarioNivel");
        return string.Equals(nivel, "ADMIN", StringComparison.OrdinalIgnoreCase)
            || string.Equals(nivel, "OPERACIONAL", StringComparison.OrdinalIgnoreCase);
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

            var pontos =
                await _pontoService.ConsultarAsync(
                    token,
                    id,
                    cancellationToken: cancellationToken);

            var model = new FuncionarioDetalhesViewModel
            {
                Funcionario = funcionario,
                Pontos = pontos
                    .OrderByDescending(p => p.DataPonto)
                    .ToList()
            };

            return View(model);
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

        if (!UsuarioEhAdminOuOperacional())
        {
            TempData["Erro"] = "Acesso restrito a administradores e operacionais.";
            return RedirectToAction("Index", "Dashboard");
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

        if (!UsuarioEhAdminOuOperacional())
        {
            TempData["Erro"] = "Acesso restrito a administradores e operacionais.";
            return RedirectToAction("Index", "Dashboard");
        }

        // Só o ADMIN escolhe o nível. Para o operacional, o cadastro sempre
        // sai como FUNCIONARIO, mesmo que o formulário tenha sido adulterado.
        if (!UsuarioEhAdmin())
        {
            model.NivelAcesso = "FUNCIONARIO";
            ModelState.Remove(nameof(model.NivelAcesso));
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

            TempData["Sucesso"] = "Funcionário cadastrado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException ex)
        {
            var mensagem = ex.Message;

            if (mensagem.Contains("E-mail", StringComparison.OrdinalIgnoreCase)
                || mensagem.Contains("CPF", StringComparison.OrdinalIgnoreCase)
                || mensagem.Contains("já cadastrado", StringComparison.OrdinalIgnoreCase))
            {
                mensagem = "Já existe um funcionário cadastrado com este e-mail ou CPF.";
            }
            else if (mensagem.Contains("senha", StringComparison.OrdinalIgnoreCase))
            {
                mensagem = "A senha informada não atende aos requisitos do sistema.";
            }
            else
            {
                mensagem = "Não foi possível cadastrar o funcionário. Verifique os dados e tente novamente.";
            }

            ModelState.AddModelError(string.Empty, mensagem);
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
            DataAdmissao = funcionario.DataAdmissao,
            NivelAcesso = funcionario.NivelAcesso
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

        // Só o ADMIN altera o nível. Para o operacional, nulo mantém o atual.
        if (!UsuarioEhAdmin())
        {
            model.NivelAcesso = null;
            ModelState.Remove(nameof(model.NivelAcesso));
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

        // Ativar e desativar é função exclusiva do ADMIN. O botão já some para
        // o operacional na lista, mas a rota também precisa recusar o POST.
        if (!UsuarioEhAdmin())
        {
            TempData["Erro"] = "Somente administradores podem ativar ou desativar funcionários.";
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