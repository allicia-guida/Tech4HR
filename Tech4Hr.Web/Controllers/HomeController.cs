using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Tech4Hr.Web.Models;

namespace Tech4Hr.Web.Controllers;

public class HomeController : Controller
{
    // A raiz do site leva cada pessoa para a própria tela: o funcionário (e o
    // operacional) para o registro de ponto, o administrador para o painel e
    // quem não entrou ainda para o login.
    public IActionResult Index()
    {
        if (!string.IsNullOrWhiteSpace(HttpContext.Session.GetString("FuncionarioAuthToken")))
        {
            return RedirectToAction("Index", "MeuPonto");
        }

        if (!string.IsNullOrWhiteSpace(HttpContext.Session.GetString("AuthToken")))
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return RedirectToAction("Login", "Auth");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
