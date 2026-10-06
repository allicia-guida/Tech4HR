using System.Net;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Tech4Hr.API.Data;
using Tech4Hr.API.DTOs;
using Tech4Hr.API.Models;
using Tech4Hr.API.Services;
using Tech4Hr.Web.Models;
using Tech4Hr.Web.Services;
using Xunit;
using ApiAuthController = Tech4Hr.API.Controllers.AuthController;

namespace Tech4Hr.Tests;

// Limite de tentativas de login por conta: 5 senhas erradas em 10 minutos
// bloqueiam a conta por 10 minutos, mesmo que a senha certa chegue.
public class LimiteDeLoginTests
{
    private sealed class RelogioAjustavel : TimeProvider
    {
        private DateTimeOffset _agora = new(2026, 10, 5, 12, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => _agora;

        public void Avancar(TimeSpan tempo) => _agora += tempo;
    }

    private static Tech4HrDbContext CriarContexto() =>
        new(new DbContextOptionsBuilder<Tech4HrDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static IConfiguration CriarConfiguracaoJwt() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "Tech4HrTests",
                ["Jwt:Audience"] = "Tech4HrAudience",
                ["Jwt:Key"] = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes("12345678901234567890123456789012")),
                ["Jwt:ExpirationMinutes"] = "60"
            })
            .Build();

    private static Funcionario NovoFuncionario(string email) => new()
    {
        Nome = "Nome",
        Sobrenome = "Teste",
        EmailCorporativo = email,
        CPF = "33333333333",
        SenhaHash = new PasswordHasher<Funcionario>().HashPassword(null!, "Senha@123"),
        DataAdmissao = new DateTime(2026, 1, 1),
        Ativo = true,
        NivelAcesso = "FUNCIONARIO"
    };

    private static Task<IActionResult> LoginFuncionario(
        ApiAuthController controller, string email, string senha) =>
        controller.LoginFuncionario(new LoginFuncionarioDto
        {
            EmailCorporativo = email,
            Senha = senha
        });

    private static string? LerMensagem(IActionResult resultado)
    {
        var objeto = Assert.IsType<ObjectResult>(resultado);
        return objeto.Value?.GetType().GetProperty("message")?.GetValue(objeto.Value) as string;
    }

    // ---------- Regra de contagem ----------

    [Fact]
    public void QuatroFalhas_NaoBloqueiam()
    {
        var limite = new LimiteDeTentativasDeLogin(new RelogioAjustavel());
        var chave = LimiteDeTentativasDeLogin.Chave("USUARIO", "a@empresa.com");

        for (var i = 0; i < 4; i++) limite.RegistrarFalha(chave);

        Assert.Null(limite.TempoRestanteDeBloqueio(chave));
    }

    [Fact]
    public void CincoFalhas_BloqueiamPorDezMinutos()
    {
        var limite = new LimiteDeTentativasDeLogin(new RelogioAjustavel());
        var chave = LimiteDeTentativasDeLogin.Chave("USUARIO", "a@empresa.com");

        for (var i = 0; i < 5; i++) limite.RegistrarFalha(chave);

        Assert.Equal(TimeSpan.FromMinutes(10), limite.TempoRestanteDeBloqueio(chave));
    }

    [Fact]
    public void DepoisDoBloqueio_ContaVoltaAAceitarEAContagemRecomeca()
    {
        var relogio = new RelogioAjustavel();
        var limite = new LimiteDeTentativasDeLogin(relogio);
        var chave = LimiteDeTentativasDeLogin.Chave("USUARIO", "a@empresa.com");

        for (var i = 0; i < 5; i++) limite.RegistrarFalha(chave);
        relogio.Avancar(TimeSpan.FromMinutes(10) + TimeSpan.FromSeconds(1));

        Assert.Null(limite.TempoRestanteDeBloqueio(chave));

        // Uma falha logo depois de liberada não bloqueia de novo.
        limite.RegistrarFalha(chave);
        Assert.Null(limite.TempoRestanteDeBloqueio(chave));
    }

    [Fact]
    public void TentativasDuranteOBloqueio_NaoOProlongam()
    {
        var relogio = new RelogioAjustavel();
        var limite = new LimiteDeTentativasDeLogin(relogio);
        var chave = LimiteDeTentativasDeLogin.Chave("USUARIO", "a@empresa.com");

        for (var i = 0; i < 5; i++) limite.RegistrarFalha(chave);
        relogio.Avancar(TimeSpan.FromMinutes(5));
        limite.RegistrarFalha(chave);
        relogio.Avancar(TimeSpan.FromMinutes(5) + TimeSpan.FromSeconds(1));

        Assert.Null(limite.TempoRestanteDeBloqueio(chave));
    }

    [Fact]
    public void FalhasForaDaJanela_NaoSeAcumulam()
    {
        var relogio = new RelogioAjustavel();
        var limite = new LimiteDeTentativasDeLogin(relogio);
        var chave = LimiteDeTentativasDeLogin.Chave("USUARIO", "a@empresa.com");

        for (var i = 0; i < 3; i++) limite.RegistrarFalha(chave);
        relogio.Avancar(TimeSpan.FromMinutes(11));
        for (var i = 0; i < 3; i++) limite.RegistrarFalha(chave);

        Assert.Null(limite.TempoRestanteDeBloqueio(chave));
    }

    [Fact]
    public void Sucesso_ZeraAContagem()
    {
        var limite = new LimiteDeTentativasDeLogin(new RelogioAjustavel());
        var chave = LimiteDeTentativasDeLogin.Chave("USUARIO", "a@empresa.com");

        for (var i = 0; i < 4; i++) limite.RegistrarFalha(chave);
        limite.RegistrarSucesso(chave);
        for (var i = 0; i < 4; i++) limite.RegistrarFalha(chave);

        Assert.Null(limite.TempoRestanteDeBloqueio(chave));
    }

    [Fact]
    public void ContasEmailsETiposDiferentes_SaoIndependentes()
    {
        var limite = new LimiteDeTentativasDeLogin(new RelogioAjustavel());
        var alvo = LimiteDeTentativasDeLogin.Chave("USUARIO", "a@empresa.com");

        for (var i = 0; i < 5; i++) limite.RegistrarFalha(alvo);

        Assert.NotNull(limite.TempoRestanteDeBloqueio(alvo));
        Assert.Null(limite.TempoRestanteDeBloqueio(LimiteDeTentativasDeLogin.Chave("USUARIO", "b@empresa.com")));
        Assert.Null(limite.TempoRestanteDeBloqueio(LimiteDeTentativasDeLogin.Chave("FUNCIONARIO", "a@empresa.com")));
    }

    [Fact]
    public void Chave_IgnoraMaiusculasEEspacosDoEmail()
    {
        Assert.Equal(
            LimiteDeTentativasDeLogin.Chave("USUARIO", "a@empresa.com"),
            LimiteDeTentativasDeLogin.Chave("USUARIO", "  A@Empresa.COM "));
    }

    // ---------- API: login ----------

    [Fact]
    public async Task LoginFuncionario_DepoisDeCincoSenhasErradas_BloqueiaMesmoComASenhaCerta()
    {
        await using var contexto = CriarContexto();
        contexto.Funcionarios.Add(NovoFuncionario("maria@empresa.com"));
        await contexto.SaveChangesAsync();

        var controller = new ApiAuthController(
            contexto, CriarConfiguracaoJwt(), new LimiteDeTentativasDeLogin(new RelogioAjustavel()));

        for (var i = 0; i < 5; i++)
        {
            Assert.IsType<UnauthorizedObjectResult>(
                await LoginFuncionario(controller, "maria@empresa.com", "errada"));
        }

        var bloqueado = await LoginFuncionario(controller, "maria@empresa.com", "Senha@123");

        Assert.Equal(429, Assert.IsType<ObjectResult>(bloqueado).StatusCode);
        Assert.Equal("Muitas tentativas de login. Tente novamente em 10 minutos.", LerMensagem(bloqueado));
    }

    [Fact]
    public async Task LoginFuncionario_SenhaCertaZeraAsFalhas()
    {
        await using var contexto = CriarContexto();
        contexto.Funcionarios.Add(NovoFuncionario("maria@empresa.com"));
        await contexto.SaveChangesAsync();

        var controller = new ApiAuthController(
            contexto, CriarConfiguracaoJwt(), new LimiteDeTentativasDeLogin(new RelogioAjustavel()));

        for (var i = 0; i < 4; i++) await LoginFuncionario(controller, "maria@empresa.com", "errada");
        Assert.IsType<OkObjectResult>(await LoginFuncionario(controller, "maria@empresa.com", "Senha@123"));

        for (var i = 0; i < 4; i++) await LoginFuncionario(controller, "maria@empresa.com", "errada");
        Assert.IsType<OkObjectResult>(await LoginFuncionario(controller, "maria@empresa.com", "Senha@123"));
    }

    [Fact]
    public async Task LoginFuncionario_EmailInexistente_TambemBloqueia_ParaNaoRevelarContas()
    {
        await using var contexto = CriarContexto();

        var controller = new ApiAuthController(
            contexto, CriarConfiguracaoJwt(), new LimiteDeTentativasDeLogin(new RelogioAjustavel()));

        for (var i = 0; i < 5; i++)
        {
            Assert.IsType<UnauthorizedObjectResult>(
                await LoginFuncionario(controller, "ninguem@empresa.com", "qualquer"));
        }

        var bloqueado = await LoginFuncionario(controller, "ninguem@empresa.com", "qualquer");

        Assert.Equal(429, Assert.IsType<ObjectResult>(bloqueado).StatusCode);
    }

    [Fact]
    public async Task LoginAdministrativo_DepoisDeCincoSenhasErradas_Bloqueia()
    {
        await using var contexto = CriarContexto();
        contexto.Usuarios.Add(new Usuario
        {
            Nome = "Admin",
            Sobrenome = "Teste",
            Email = "admin@empresa.com",
            SenhaHash = new PasswordHasher<Usuario>().HashPassword(null!, "Senha@123"),
            NivelUsuario = "ADMIN",
            Ativo = true
        });
        await contexto.SaveChangesAsync();

        var controller = new ApiAuthController(
            contexto, CriarConfiguracaoJwt(), new LimiteDeTentativasDeLogin(new RelogioAjustavel()));

        for (var i = 0; i < 5; i++)
        {
            await controller.Login(new LoginUsuarioDto { Email = "admin@empresa.com", Senha = "errada" });
        }

        var bloqueado = await controller.Login(
            new LoginUsuarioDto { Email = "admin@empresa.com", Senha = "Senha@123" });

        Assert.Equal(429, Assert.IsType<ObjectResult>(bloqueado).StatusCode);
    }

    // ---------- Web: mensagem do bloqueio ----------

    private sealed class RespostaFixaHandler(HttpStatusCode status, string corpo) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(corpo, Encoding.UTF8, "application/json")
            });
    }

    private sealed class FabricaDeClienteFalsa(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) =>
            new(handler) { BaseAddress = new Uri("http://api.teste/") };
    }

    [Fact]
    public async Task Web_LoginFuncionario_Com429_MostraAMensagemDaApi()
    {
        var servico = new AuthService(new FabricaDeClienteFalsa(new RespostaFixaHandler(
            HttpStatusCode.TooManyRequests,
            "{\"message\":\"Muitas tentativas de login. Tente novamente em 7 minutos.\"}")));

        var resultado = await servico.LoginFuncionarioAsync(
            new LoginViewModel { Email = "maria@empresa.com", Senha = "x" });

        Assert.False(resultado.IsSuccess);
        Assert.Equal("Muitas tentativas de login. Tente novamente em 7 minutos.", resultado.ErrorMessage);
    }

    [Fact]
    public async Task Web_LoginAdministrativo_Com429SemCorpoValido_UsaMensagemPadrao()
    {
        var servico = new AuthService(new FabricaDeClienteFalsa(
            new RespostaFixaHandler(HttpStatusCode.TooManyRequests, "isto nao e json")));

        var resultado = await servico.LoginAsync(
            new LoginViewModel { Email = "admin@empresa.com", Senha = "x" });

        Assert.False(resultado.IsSuccess);
        Assert.Equal(
            "Muitas tentativas de login. Tente novamente em alguns minutos.",
            resultado.ErrorMessage);
    }

}
