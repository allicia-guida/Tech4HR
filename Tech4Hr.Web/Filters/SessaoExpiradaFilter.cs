using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Tech4Hr.Web.Services;

namespace Tech4Hr.Web.Filters;

/// <summary>
/// Roda em volta de toda ação. Se durante a ação a API recusou o token
/// (conta desativada ou sessão expirada), limpa a sessão e redireciona para o
/// login com um aviso, em vez de deixar a tela mostrando "Status: 401".
/// </summary>
public sealed class SessaoExpiradaFilter : IAsyncActionFilter
{
    public const string Mensagem =
        "Sua sessão expirou ou a conta foi desativada. Entre novamente.";

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var executada = await next();

        if (!context.HttpContext.Items.ContainsKey(SessaoExpiradaHandler.ChaveDoItem))
        {
            return;
        }

        context.HttpContext.Session.Clear();

        if (context.Controller is Controller controller)
        {
            controller.TempData["Erro"] = Mensagem;
        }

        // Se a ação estourou por causa do 401, a exceção já foi tratada aqui.
        executada.ExceptionHandled = true;
        executada.Result = new RedirectToActionResult("Login", "Auth", null);
    }
}
