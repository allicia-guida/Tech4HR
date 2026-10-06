using Microsoft.AspNetCore.Mvc;
using Tech4Hr.Web.Models;
using Tech4Hr.Web.Services;

namespace Tech4Hr.Web.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var resultado = await _authService.LoginAsync(
            model,
            cancellationToken);

        if (!resultado.IsSuccess ||
            resultado.User is null ||
            string.IsNullOrWhiteSpace(resultado.Token))
        {
            ModelState.AddModelError(
                string.Empty,
                resultado.ErrorMessage ?? "Não foi possível realizar o login.");

            return View(model);
        }

        HttpContext.Session.Clear();

        HttpContext.Session.SetString(
            "AuthToken",
            resultado.Token);

        HttpContext.Session.SetString(
            "TipoConta",
            resultado.User.NivelUsuario);

        HttpContext.Session.SetString(
            "UsuarioNome",
            $"{resultado.User.Nome} {resultado.User.Sobrenome}");

        HttpContext.Session.SetString(
            "UsuarioEmail",
            resultado.User.Email);

        HttpContext.Session.SetString(
            "UsuarioNivel",
            resultado.User.NivelUsuario);

        HttpContext.Session.SetInt32(
            "UsuarioId",
            resultado.User.IdUsuario);

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginFuncionario(
        LoginViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("Login", model);
        }

        var resultado = await _authService.LoginFuncionarioAsync(
            model,
            cancellationToken);

        if (!resultado.IsSuccess ||
            resultado.User is null ||
            string.IsNullOrWhiteSpace(resultado.Token))
        {
            ModelState.AddModelError(
                string.Empty,
                resultado.ErrorMessage ?? "Não foi possível realizar o login do funcionário.");

            return View("Login", model);
        }

        HttpContext.Session.Clear();

        HttpContext.Session.SetString(
            "FuncionarioAuthToken",
            resultado.Token);

        HttpContext.Session.SetString(
            "TipoConta",
            "FUNCIONARIO");

        HttpContext.Session.SetInt32(
            "FuncionarioId",
            resultado.User.IdFuncionario);

        HttpContext.Session.SetString(
            "FuncionarioNome",
            $"{resultado.User.Nome} {resultado.User.Sobrenome}".Trim());

        HttpContext.Session.SetString(
            "FuncionarioEmail",
            resultado.User.EmailCorporativo);

        // O operacional é um funcionário com permissão extra: usa o mesmo token
        // para bater ponto e para as telas de gestão, que leem AuthToken e
        // UsuarioNivel. O ADMIN continua entrando só pelo login administrativo.
        if (resultado.User.EhOperacional)
        {
            HttpContext.Session.SetString("AuthToken", resultado.Token);
            HttpContext.Session.SetString("UsuarioNivel", "OPERACIONAL");

            HttpContext.Session.SetString(
                "UsuarioNome",
                $"{resultado.User.Nome} {resultado.User.Sobrenome}".Trim());

            HttpContext.Session.SetString(
                "UsuarioEmail",
                resultado.User.EmailCorporativo);
        }

        return RedirectToAction("Index", "MeuPonto");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();

        return RedirectToAction(nameof(Login));
    }
}