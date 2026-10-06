using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Tech4Hr.API.Data;
using Tech4Hr.API.Models;
using Tech4Hr.API.Services;
using Tech4Hr.Web.Filters;
using Tech4Hr.Web.Models;
using Tech4Hr.Web.Services;
using Xunit;
using ApiFuncionariosController = Tech4Hr.API.Controllers.FuncionariosController;
using ApiUsuariosController = Tech4Hr.API.Controllers.UsuariosController;
using WebFuncionariosController = Tech4Hr.Web.Controllers.FuncionariosController;

namespace Tech4Hr.Tests;

public class DesativacaoTests
{
    private static Tech4HrDbContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<Tech4HrDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new Tech4HrDbContext(options);
    }

    private static ClaimsPrincipal CriarToken(string tipoConta, string papel, string id)
    {
        var identidade = new ClaimsIdentity(
            new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, id),
                new Claim("tipo_conta", tipoConta),
                new Claim("role", papel)
            },
            "teste");

        return new ClaimsPrincipal(identidade);
    }

    // ---------- API: a conta do token precisa continuar valendo ----------

    [Fact]
    public async Task Validador_UsuarioAtivoComMesmoPerfil_EhAceito()
    {
        await using var contexto = CriarContexto();

        contexto.Usuarios.Add(new Usuario
        {
            Nome = "Ana", Sobrenome = "Admin", Email = "ana@t.com",
            SenhaHash = "h", NivelUsuario = "ADMIN", Ativo = true
        });
        await contexto.SaveChangesAsync();

        var motivo = await new ValidadorDeConta(contexto)
            .VerificarAsync(CriarToken("USUARIO", "ADMIN", "1"));

        Assert.Null(motivo);
    }

    [Fact]
    public async Task Validador_UsuarioDesativadoDepoisDeReceberOToken_EhRecusado()
    {
        await using var contexto = CriarContexto();

        contexto.Usuarios.Add(new Usuario
        {
            Nome = "Ana", Sobrenome = "Admin", Email = "ana@t.com",
            SenhaHash = "h", NivelUsuario = "ADMIN", Ativo = true
        });
        contexto.Usuarios.Add(new Usuario
        {
            Nome = "Beto", Sobrenome = "Op", Email = "beto@t.com",
            SenhaHash = "h", NivelUsuario = "OPERACIONAL", Ativo = true
        });
        await contexto.SaveChangesAsync();

        // O token do Beto foi emitido com a conta ativa. Depois o ADMIN a desativa.
        var beto = await contexto.Usuarios.SingleAsync(u => u.Email == "beto@t.com");
        beto.Ativo = false;
        await contexto.SaveChangesAsync();

        var motivo = await new ValidadorDeConta(contexto)
            .VerificarAsync(CriarToken("USUARIO", "OPERACIONAL", beto.IdUsuario.ToString()));

        Assert.Equal("Conta desativada.", motivo);
    }

    [Fact]
    public async Task Validador_PerfilRebaixadoDepoisDeReceberOToken_EhRecusado()
    {
        await using var contexto = CriarContexto();

        contexto.Usuarios.AddRange(
            new Usuario
            {
                Nome = "Ana", Sobrenome = "Admin", Email = "ana@t.com",
                SenhaHash = "h", NivelUsuario = "ADMIN", Ativo = true
            },
            new Usuario
            {
                Nome = "Caio", Sobrenome = "Rebaixado", Email = "caio@t.com",
                SenhaHash = "h", NivelUsuario = "OPERACIONAL", Ativo = true
            });
        await contexto.SaveChangesAsync();

        var caio = await contexto.Usuarios.SingleAsync(u => u.Email == "caio@t.com");

        // Token antigo ainda diz ADMIN, mas no banco ele já é OPERACIONAL.
        var motivo = await new ValidadorDeConta(contexto)
            .VerificarAsync(CriarToken("USUARIO", "ADMIN", caio.IdUsuario.ToString()));

        Assert.NotNull(motivo);
        Assert.Contains("perfil", motivo!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Validador_UsuarioInexistente_EhRecusado()
    {
        await using var contexto = CriarContexto();

        var motivo = await new ValidadorDeConta(contexto)
            .VerificarAsync(CriarToken("USUARIO", "ADMIN", "999"));

        Assert.Equal("Conta desativada.", motivo);
    }

    [Fact]
    public async Task Validador_FuncionarioAtivoEhAceitoEInativoEhRecusado()
    {
        await using var contexto = CriarContexto();

        contexto.Funcionarios.AddRange(
            new Funcionario
            {
                Nome = "Dora", Sobrenome = "Ativa", EmailCorporativo = "dora@e.com",
                CPF = "11111111111", SenhaHash = "h",
                DataAdmissao = new DateTime(2026, 1, 1), Ativo = true
            },
            new Funcionario
            {
                Nome = "Edu", Sobrenome = "Inativo", EmailCorporativo = "edu@e.com",
                CPF = "22222222222", SenhaHash = "h",
                DataAdmissao = new DateTime(2026, 1, 1), Ativo = false
            });
        await contexto.SaveChangesAsync();

        var dora = await contexto.Funcionarios.SingleAsync(f => f.EmailCorporativo == "dora@e.com");
        var edu = await contexto.Funcionarios.SingleAsync(f => f.EmailCorporativo == "edu@e.com");
        var validador = new ValidadorDeConta(contexto);

        Assert.Null(await validador.VerificarAsync(
            CriarToken("FUNCIONARIO", "FUNCIONARIO", dora.IdFuncionario.ToString())));

        Assert.Equal("Conta desativada.", await validador.VerificarAsync(
            CriarToken("FUNCIONARIO", "FUNCIONARIO", edu.IdFuncionario.ToString())));
    }

    [Theory]
    [InlineData("OUTRO", "1")]
    [InlineData("USUARIO", "abc")]
    public async Task Validador_TokenMalFormado_EhRecusado(string tipoConta, string id)
    {
        await using var contexto = CriarContexto();

        var motivo = await new ValidadorDeConta(contexto)
            .VerificarAsync(CriarToken(tipoConta, "ADMIN", id));

        Assert.NotNull(motivo);
    }

    // ---------- API: só ADMIN ativa e desativa ----------

    [Theory]
    [InlineData(typeof(ApiUsuariosController))]
    [InlineData(typeof(ApiFuncionariosController))]
    public void AlterarStatus_NaApi_ExigeSomenteAdmin(Type controller)
    {
        var metodo = controller.GetMethod("AlterarStatus")!;

        var autorizacoes = metodo.GetCustomAttributes<AuthorizeAttribute>().ToList();

        Assert.Single(autorizacoes);
        Assert.Equal("ADMIN", autorizacoes[0].Roles);
    }

    // ---------- Web: o operacional não pode desativar funcionário ----------

    private static (WebFuncionariosController Controller, ServicoFuncionarioFalso Servico)
        CriarControllerWeb(string? nivel)
    {
        var contextoHttp = new DefaultHttpContext { Session = new SessaoFalsa() };
        contextoHttp.Session.SetString("AuthToken", "token-de-teste");

        if (nivel is not null)
        {
            contextoHttp.Session.SetString("UsuarioNivel", nivel);
        }

        var servico = new ServicoFuncionarioFalso();

        var controller = new WebFuncionariosController(servico, null!)
        {
            ControllerContext = new ControllerContext { HttpContext = contextoHttp },
            TempData = new TempDataDictionary(contextoHttp, new ProvedorTempDataFalso())
        };

        return (controller, servico);
    }

    [Fact]
    public async Task WebAlterarStatus_Operacional_EhBarradoENaoChamaAApi()
    {
        var (controller, servico) = CriarControllerWeb("OPERACIONAL");

        var resultado = await controller.AlterarStatus(1, false, CancellationToken.None);

        var redirecionamento = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Dashboard", redirecionamento.ControllerName);
        Assert.Equal(0, servico.ChamadasAlterarStatus);
        Assert.NotNull(controller.TempData["Erro"]);
    }

    [Fact]
    public async Task WebAlterarStatus_Admin_ChamaAApi()
    {
        var (controller, servico) = CriarControllerWeb("ADMIN");

        var resultado = await controller.AlterarStatus(1, false, CancellationToken.None);

        var redirecionamento = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Index", redirecionamento.ActionName);
        Assert.Equal(1, servico.ChamadasAlterarStatus);
    }

    // ---------- Web: API recusou o token, volta ao login ----------

    private sealed class RespostaFixaHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;

        public RespostaFixaHandler(HttpStatusCode status) => _status = status;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(_status));
    }

    private static async Task<DefaultHttpContext> EnviarComHandlerAsync(
        HttpStatusCode status, bool comToken)
    {
        var contextoHttp = new DefaultHttpContext();
        var handler = new SessaoExpiradaHandler(new HttpContextAccessor { HttpContext = contextoHttp })
        {
            InnerHandler = new RespostaFixaHandler(status)
        };

        using var cliente = new HttpClient(handler) { BaseAddress = new Uri("http://api.local/") };
        using var requisicao = new HttpRequestMessage(HttpMethod.Get, "api/x");

        if (comToken)
        {
            requisicao.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "t");
        }

        await cliente.SendAsync(requisicao);
        return contextoHttp;
    }

    [Fact]
    public async Task Handler_401ComToken_MarcaSessaoExpirada()
    {
        var contexto = await EnviarComHandlerAsync(HttpStatusCode.Unauthorized, comToken: true);

        Assert.True(contexto.Items.ContainsKey(SessaoExpiradaHandler.ChaveDoItem));
    }

    [Fact]
    public async Task Handler_401SemToken_NaoMarca()
    {
        // Senha errada no login devolve 401 sem token enviado: não é sessão expirada.
        var contexto = await EnviarComHandlerAsync(HttpStatusCode.Unauthorized, comToken: false);

        Assert.False(contexto.Items.ContainsKey(SessaoExpiradaHandler.ChaveDoItem));
    }

    [Fact]
    public async Task Handler_Resposta200_NaoMarca()
    {
        var contexto = await EnviarComHandlerAsync(HttpStatusCode.OK, comToken: true);

        Assert.False(contexto.Items.ContainsKey(SessaoExpiradaHandler.ChaveDoItem));
    }

    private sealed class ControllerDeTeste : Controller
    {
    }

    private static async Task<(ActionExecutedContext Executado, DefaultHttpContext Http, ControllerDeTeste Controller)>
        ExecutarFiltroAsync(bool sessaoExpirou)
    {
        var contextoHttp = new DefaultHttpContext { Session = new SessaoFalsa() };
        contextoHttp.Session.SetString("AuthToken", "token-de-teste");

        if (sessaoExpirou)
        {
            contextoHttp.Items[SessaoExpiradaHandler.ChaveDoItem] = true;
        }

        var controller = new ControllerDeTeste
        {
            TempData = new TempDataDictionary(contextoHttp, new ProvedorTempDataFalso())
        };

        var contextoAcao = new ActionContext(contextoHttp, new RouteData(), new ActionDescriptor());
        var filtros = new List<IFilterMetadata>();

        var executando = new ActionExecutingContext(
            contextoAcao, filtros, new Dictionary<string, object?>(), controller);

        var executado = new ActionExecutedContext(contextoAcao, filtros, controller)
        {
            Result = new ViewResult()
        };

        await new SessaoExpiradaFilter().OnActionExecutionAsync(
            executando, () => Task.FromResult(executado));

        return (executado, contextoHttp, controller);
    }

    [Fact]
    public async Task Filtro_SessaoExpirada_LimpaSessaoEVoltaAoLogin()
    {
        var (executado, http, controller) = await ExecutarFiltroAsync(sessaoExpirou: true);

        var redirecionamento = Assert.IsType<RedirectToActionResult>(executado.Result);
        Assert.Equal("Login", redirecionamento.ActionName);
        Assert.Equal("Auth", redirecionamento.ControllerName);
        Assert.Null(http.Session.GetString("AuthToken"));
        Assert.Equal(SessaoExpiradaFilter.Mensagem, controller.TempData["Erro"]);
    }

    [Fact]
    public async Task Filtro_SessaoNormal_NaoMexeNoResultado()
    {
        var (executado, http, _) = await ExecutarFiltroAsync(sessaoExpirou: false);

        Assert.IsType<ViewResult>(executado.Result);
        Assert.Equal("token-de-teste", http.Session.GetString("AuthToken"));
    }
}
