using Microsoft.AspNetCore.Mvc;
using Tech4Hr.Web.Models;
using Tech4Hr.Web.Services;

namespace Tech4Hr.Web.Controllers;

public class MeuPontoController : Controller
{
    private readonly IPontoService _pontoService;

    public MeuPontoController(IPontoService pontoService)
    {
        _pontoService = pontoService;
    }

    private bool UsuarioFuncionarioLogado()
    {
        var token = HttpContext.Session.GetString("FuncionarioAuthToken");
        var tipoConta = HttpContext.Session.GetString("TipoConta");

        return !string.IsNullOrWhiteSpace(token)
            && string.Equals(tipoConta, "FUNCIONARIO", StringComparison.OrdinalIgnoreCase);
    }

    private static string? ObterProximoTipoRegistro(PontoApiResponse? pontoHoje)
    {
        if (pontoHoje is null)
        {
            return "ENTRADA";
        }

        if (!pontoHoje.Entrada.HasValue)
        {
            return "ENTRADA";
        }

        if (!pontoHoje.SaidaAlmoco.HasValue)
        {
            return "SAIDA_ALMOCO";
        }

        if (!pontoHoje.EntradaAlmoco.HasValue)
        {
            return "ENTRADA_ALMOCO";
        }

        if (!pontoHoje.Saida.HasValue)
        {
            return "SAIDA";
        }

        return null;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (!UsuarioFuncionarioLogado())
        {
            return RedirectToAction("Login", "Auth");
        }

        var token = HttpContext.Session.GetString("FuncionarioAuthToken");
        var nome = HttpContext.Session.GetString("FuncionarioNome");

        try
        {
            var hoje = DateTime.Today;
            var registros = await _pontoService.ConsultarMeusPontosAsync(
                token!,
                hoje,
                hoje,
                cancellationToken);

            var pontoHoje = registros
                .OrderByDescending(p => p.DataPonto)
                .FirstOrDefault();

            var proximoTipo = ObterProximoTipoRegistro(pontoHoje);

            var model = new MeuPontoViewModel
            {
                NomeFuncionario = string.IsNullOrWhiteSpace(nome) ? "Funcionário" : nome,
                Registros = registros
                    .OrderByDescending(p => p.DataPonto)
                    .ToList(),
                DataAtual = DateTime.Now,
                PontoHoje = pontoHoje,
                ProximoTipoRegistro = proximoTipo ?? string.Empty,
                PodeRegistrarPonto = !string.IsNullOrWhiteSpace(proximoTipo),
                UltimoRegistro = pontoHoje is null ? null : pontoHoje.DataPonto.ToString("dd/MM/yyyy")
            };

            return View(model);
        }
        catch (HttpRequestException ex)
        {
            TempData["Erro"] = ex.Message;

            return View(new MeuPontoViewModel
            {
                NomeFuncionario = string.IsNullOrWhiteSpace(nome) ? "Funcionário" : nome,
                DataAtual = DateTime.Now,
                PodeRegistrarPonto = false,
                ProximoTipoRegistro = string.Empty
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Historico(
        DateTime? dataInicio,
        DateTime? dataFim,
        CancellationToken cancellationToken)
    {
        if (!UsuarioFuncionarioLogado())
        {
            return RedirectToAction("Login", "Auth");
        }

        var token = HttpContext.Session.GetString("FuncionarioAuthToken");

        try
        {
            var registros = await _pontoService.ConsultarMeusPontosAsync(
                token!,
                dataInicio,
                dataFim,
                cancellationToken);

            var model = new HistoricoMeuPontoViewModel
            {
                DataInicio = dataInicio,
                DataFim = dataFim,
                Registros = registros
                    .OrderByDescending(p => p.DataPonto)
                    .ToList()
            };

            return View(model);
        }
        catch (HttpRequestException ex)
        {
            TempData["Erro"] = ex.Message;

            return View(new HistoricoMeuPontoViewModel
            {
                DataInicio = dataInicio,
                DataFim = dataFim
            });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarPonto(CancellationToken cancellationToken)
    {
        if (!UsuarioFuncionarioLogado())
        {
            return RedirectToAction("Login", "Auth");
        }

        var token = HttpContext.Session.GetString("FuncionarioAuthToken");

        try
        {
            var hoje = DateTime.Today;
            var registrosHoje = await _pontoService.ConsultarMeusPontosAsync(
                token!,
                hoje,
                hoje,
                cancellationToken);

            var pontoHoje = registrosHoje
                .OrderByDescending(p => p.DataPonto)
                .FirstOrDefault();

            var tipoRegistro = ObterProximoTipoRegistro(pontoHoje);

            if (string.IsNullOrWhiteSpace(tipoRegistro))
            {
                TempData["Erro"] = "Jornada registrada por completo hoje.";
                return RedirectToAction(nameof(Index));
            }

            var resultado = await _pontoService.RegistrarAsync(
                token!,
                tipoRegistro,
                cancellationToken);

            TempData["Sucesso"] = resultado.Mensagem;
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException ex)
        {
            TempData["Erro"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
