using Microsoft.AspNetCore.Mvc;

namespace Tech4Hr.Web.Controllers;

public class FuncionariosController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Cadastrar()
    {
        return View();
    }

    public IActionResult Editar(int id)
    {
        return View();
    }
}
