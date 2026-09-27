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

        HttpContext.Session.SetString(
            "AuthToken",
            resultado.Token);

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
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();

        return RedirectToAction(nameof(Login));
    }
}