using Microsoft.AspNetCore.Mvc;

namespace Tech4Hr.Web.Controllers;

public class DashboardController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
