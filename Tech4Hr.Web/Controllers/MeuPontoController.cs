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
            // O dia e a hora vêm da API (horário de Brasília), não do relógio
            // do servidor Web nem do aparelho de quem está batendo o ponto.
            var hoje = await _pontoService.ObterHojeAsync(token!, cancellationToken);

            var pontoHoje = hoje.Ponto;
            var proximoTipo = hoje.ProximoTipoRegistro;

            var model = new MeuPontoViewModel
            {
                NomeFuncionario = string.IsNullOrWhiteSpace(nome) ? "Funcionário" : nome,
                Registros = pontoHoje is null
                    ? Array.Empty<PontoApiResponse>()
                    : new[] { pontoHoje },
                DataAtual = hoje.Agora.DateTime,
                ServidorEpochMs = hoje.Agora.ToUnixTimeMilliseconds(),
                FusoHorario = hoje.FusoHorario,
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

            // Sem resposta da API não há hora oficial: a tela mostra o relógio
            // do aparelho em horário de Brasília e bloqueia o registro.
            return View(new MeuPontoViewModel
            {
                NomeFuncionario = string.IsNullOrWhiteSpace(nome) ? "Funcionário" : nome,
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
            var hoje = await _pontoService.ObterHojeAsync(token!, cancellationToken);

            var tipoRegistro = hoje.ProximoTipoRegistro;

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
