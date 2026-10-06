using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
using ApiFuncionariosController = Tech4Hr.API.Controllers.FuncionariosController;
using ApiUsuariosController = Tech4Hr.API.Controllers.UsuariosController;
using WebAuthController = Tech4Hr.Web.Controllers.AuthController;
using WebFuncionariosController = Tech4Hr.Web.Controllers.FuncionariosController;

namespace Tech4Hr.Tests;

// O operacional deixa de ser "usuário administrativo" e passa a ser um
// funcionário com nível OPERACIONAL: entra pelo login de funcionário, bate
// ponto e também gere funcionários.
public class OperacionalTests
{
    private static Tech4HrDbContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<Tech4HrDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new Tech4HrDbContext(options);
    }

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

    private static Funcionario NovoFuncionario(
        string email, string cpf, string nivel = "FUNCIONARIO", bool ativo = true) => new()
    {
        Nome = "Nome",
        Sobrenome = "Teste",
        EmailCorporativo = email,
        CPF = cpf,
        SenhaHash = new PasswordHasher<Funcionario>().HashPassword(null!, "Senha@123"),
        DataAdmissao = new DateTime(2026, 1, 1),
        Ativo = ativo,
        NivelAcesso = nivel
    };

    private static string? LerPropriedade(object? objeto, string nome) =>
        objeto?.GetType().GetProperty(nome)?.GetValue(objeto) as string;

    // Usuário autenticado com o papel indicado, como o JwtBearer monta (tipo de papel "role").
    private static ControllerContext ComoPapel(string papel)
    {
        var identidade = new ClaimsIdentity(
            new[] { new Claim("role", papel) },
            "teste",
            nameType: "name",
            roleType: "role");

        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identidade) }
        };
    }

    private static CadastrarFuncionarioDto NovoCadastro(string email, string cpf, string? nivel) => new()
    {
        Nome = "Novo",
        Sobrenome = "Colaborador",
        EmailCorporativo = email,
        Senha = "Senha@12345",
        CPF = cpf,
        DataAdmissao = new DateTime(2026, 5, 1),
        NivelAcesso = nivel
    };

    // ---------- API: login ----------

    [Fact]
    public async Task LoginFuncionario_Operacional_EmiteTokenComPapelOperacional()
    {
        await using var contexto = CriarContexto();
        contexto.Funcionarios.Add(NovoFuncionario("op@empresa.com", "11111111111", "OPERACIONAL"));
        await contexto.SaveChangesAsync();

        var controller = new ApiAuthController(contexto, CriarConfiguracaoJwt());

        var resultado = await controller.LoginFuncionario(new LoginFuncionarioDto
        {
            EmailCorporativo = "op@empresa.com",
            Senha = "Senha@123"
        });

        var ok = Assert.IsType<OkObjectResult>(resultado);
        var token = LerPropriedade(ok.Value, "token");
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Continua sendo conta de funcionário (bate ponto) mas com o papel de gestão.
        Assert.Contains(jwt.Claims, c => c.Type == "tipo_conta" && c.Value == "FUNCIONARIO");
        Assert.Contains(jwt.Claims, c => c.Type == "role" && c.Value == "OPERACIONAL");
    }

    [Fact]
    public async Task LoginFuncionario_Comum_ContinuaComPapelFuncionario()
    {
        await using var contexto = CriarContexto();
        contexto.Funcionarios.Add(NovoFuncionario("maria@empresa.com", "22222222222"));
        await contexto.SaveChangesAsync();

        var controller = new ApiAuthController(contexto, CriarConfiguracaoJwt());

        var resultado = await controller.LoginFuncionario(new LoginFuncionarioDto
        {
            EmailCorporativo = "maria@empresa.com",
            Senha = "Senha@123"
        });

        var ok = Assert.IsType<OkObjectResult>(resultado);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(LerPropriedade(ok.Value, "token"));

        Assert.Contains(jwt.Claims, c => c.Type == "role" && c.Value == "FUNCIONARIO");
    }

    [Fact]
    public async Task LoginAdministrativo_UsuarioOperacionalAntigo_EhRecusadoComMensagem()
    {
        await using var contexto = CriarContexto();
        contexto.Usuarios.Add(new Usuario
        {
            Nome = "Antigo", Sobrenome = "Operacional", Email = "antigo@empresa.com",
            SenhaHash = new PasswordHasher<Usuario>().HashPassword(null!, "Senha@123"),
            NivelUsuario = "OPERACIONAL", Ativo = true
        });
        await contexto.SaveChangesAsync();

        var controller = new ApiAuthController(contexto, CriarConfiguracaoJwt());

        var resultado = await controller.Login(new LoginUsuarioDto
        {
            Email = "antigo@empresa.com",
            Senha = "Senha@123"
        });

        var recusa = Assert.IsType<ObjectResult>(resultado);
        Assert.Equal(StatusCodes.Status403Forbidden, recusa.StatusCode);
        Assert.Contains("funcionário", LerPropriedade(recusa.Value, "message")!);
    }

    // ---------- API: quem pode criar e promover operacional ----------

    [Fact]
    public async Task Cadastrar_Admin_CriaOperacional()
    {
        await using var contexto = CriarContexto();
        var controller = new ApiFuncionariosController(contexto) { ControllerContext = ComoPapel("ADMIN") };

        var resultado = await controller.Cadastrar(NovoCadastro("op@empresa.com", "33333333333", "OPERACIONAL"));

        var criado = Assert.IsType<ObjectResult>(resultado);
        Assert.Equal(StatusCodes.Status201Created, criado.StatusCode);
        Assert.Equal("OPERACIONAL", LerPropriedade(criado.Value, "NivelAcesso"));
        Assert.Equal("OPERACIONAL", (await contexto.Funcionarios.SingleAsync()).NivelAcesso);
    }

    [Fact]
    public async Task Cadastrar_Operacional_NaoCriaOutroOperacional()
    {
        await using var contexto = CriarContexto();
        var controller = new ApiFuncionariosController(contexto) { ControllerContext = ComoPapel("OPERACIONAL") };

        var resultado = await controller.Cadastrar(NovoCadastro("op2@empresa.com", "44444444444", "OPERACIONAL"));

        Assert.IsType<ForbidResult>(resultado);
        Assert.Empty(contexto.Funcionarios);
    }

    [Fact]
    public async Task Cadastrar_Operacional_CriaFuncionarioComum()
    {
        await using var contexto = CriarContexto();
        var controller = new ApiFuncionariosController(contexto) { ControllerContext = ComoPapel("OPERACIONAL") };

        var resultado = await controller.Cadastrar(NovoCadastro("novo@empresa.com", "55555555555", null));

        var criado = Assert.IsType<ObjectResult>(resultado);
        Assert.Equal(StatusCodes.Status201Created, criado.StatusCode);
        Assert.Equal("FUNCIONARIO", (await contexto.Funcionarios.SingleAsync()).NivelAcesso);
    }

    private static EditarFuncionarioDto NovaEdicao(string email, string cpf, string? nivel) => new()
    {
        Nome = "Nome",
        Sobrenome = "Teste",
        EmailCorporativo = email,
        CPF = cpf,
        DataAdmissao = new DateTime(2026, 1, 1),
        NivelAcesso = nivel
    };

    [Fact]
    public async Task Editar_Operacional_NaoMudaONivelDeNinguem()
    {
        await using var contexto = CriarContexto();
        contexto.Funcionarios.Add(NovoFuncionario("maria@empresa.com", "66666666666"));
        await contexto.SaveChangesAsync();
        var id = (await contexto.Funcionarios.SingleAsync()).IdFuncionario;

        var controller = new ApiFuncionariosController(contexto) { ControllerContext = ComoPapel("OPERACIONAL") };

        var resultado = await controller.Editar(id, NovaEdicao("maria@empresa.com", "66666666666", "OPERACIONAL"));

        Assert.IsType<ForbidResult>(resultado);
        Assert.Equal("FUNCIONARIO", (await contexto.Funcionarios.SingleAsync()).NivelAcesso);
    }

    [Fact]
    public async Task Editar_Admin_PromoveEFuncionarioVoltaAoNivelComum()
    {
        await using var contexto = CriarContexto();
        contexto.Funcionarios.Add(NovoFuncionario("maria@empresa.com", "77777777777"));
        await contexto.SaveChangesAsync();
        var id = (await contexto.Funcionarios.SingleAsync()).IdFuncionario;

        var controller = new ApiFuncionariosController(contexto) { ControllerContext = ComoPapel("ADMIN") };

        Assert.IsType<OkObjectResult>(
            await controller.Editar(id, NovaEdicao("maria@empresa.com", "77777777777", "OPERACIONAL")));
        Assert.Equal("OPERACIONAL", (await contexto.Funcionarios.SingleAsync()).NivelAcesso);

        Assert.IsType<OkObjectResult>(
            await controller.Editar(id, NovaEdicao("maria@empresa.com", "77777777777", "FUNCIONARIO")));
        Assert.Equal("FUNCIONARIO", (await contexto.Funcionarios.SingleAsync()).NivelAcesso);
    }

    [Fact]
    public async Task Editar_Operacional_SemInformarNivel_MantemONivelAtual()
    {
        await using var contexto = CriarContexto();
        contexto.Funcionarios.Add(NovoFuncionario("colega@empresa.com", "88888888888", "OPERACIONAL"));
        await contexto.SaveChangesAsync();
        var id = (await contexto.Funcionarios.SingleAsync()).IdFuncionario;

        var controller = new ApiFuncionariosController(contexto) { ControllerContext = ComoPapel("OPERACIONAL") };

        var resultado = await controller.Editar(id, NovaEdicao("colega@empresa.com", "88888888888", null));

        Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal("OPERACIONAL", (await contexto.Funcionarios.SingleAsync()).NivelAcesso);
    }

    [Fact]
    public async Task UsuariosApi_Cadastrar_NaoAceitaMaisOperacional()
    {
        await using var contexto = CriarContexto();
        var controller = new ApiUsuariosController(contexto);

        var resultado = await controller.Cadastrar(new CadastrarUsuarioDto
        {
            Nome = "Novo",
            Sobrenome = "Operacional",
            Email = "novo@empresa.com",
            Senha = "Senha@12345",
            NivelUsuario = "OPERACIONAL"
        });

        Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Empty(contexto.Usuarios);
    }

    // ---------- API: rebaixar vale na hora ----------

    [Fact]
    public async Task Validador_OperacionalRebaixadoParaFuncionario_TokenAntigoEhRecusado()
    {
        await using var contexto = CriarContexto();
        contexto.Funcionarios.Add(NovoFuncionario("ex@empresa.com", "99999999999"));
        await contexto.SaveChangesAsync();
        var id = (await contexto.Funcionarios.SingleAsync()).IdFuncionario;

        // O token foi emitido quando ele ainda era OPERACIONAL.
        var tokenAntigo = new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, id.ToString()),
                new Claim("tipo_conta", "FUNCIONARIO"),
                new Claim("role", "OPERACIONAL")
            },
            "teste"));

        var motivo = await new ValidadorDeConta(contexto).VerificarAsync(tokenAntigo);

        Assert.NotNull(motivo);
        Assert.Contains("perfil", motivo!, StringComparison.OrdinalIgnoreCase);
    }

    // ---------- Web: o operacional entra pelo login de funcionário ----------

    private sealed class ServicoAuthFalso : IAuthService
    {
        private readonly FuncionarioAuthenticatedUser _usuario;

        public ServicoAuthFalso(string nivel) =>
            _usuario = new FuncionarioAuthenticatedUser
            {
                IdFuncionario = 10,
                Nome = "Rita",
                Sobrenome = "Lima",
                EmailCorporativo = "rita@empresa.com",
                Ativo = true,
                NivelAcesso = nivel
            };

        public Task<FuncionarioAuthLoginResult> LoginFuncionarioAsync(
            LoginViewModel model, CancellationToken cancellationToken = default) =>
            Task.FromResult(FuncionarioAuthLoginResult.Success("token-da-rita", "Bearer", null, _usuario));

        public Task<AuthLoginResult> LoginAsync(
            LoginViewModel model, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }

    private static async Task<(IActionResult Resultado, ISession Sessao)> EntrarComoFuncionarioAsync(string nivel)
    {
        var contextoHttp = new DefaultHttpContext { Session = new SessaoFalsa() };

        var controller = new WebAuthController(new ServicoAuthFalso(nivel))
        {
            ControllerContext = new ControllerContext { HttpContext = contextoHttp },
            TempData = new TempDataDictionary(contextoHttp, new ProvedorTempDataFalso())
        };

        var resultado = await controller.LoginFuncionario(
            new LoginViewModel { Email = "rita@empresa.com", Senha = "Senha@123" },
            CancellationToken.None);

        return (resultado, contextoHttp.Session);
    }

    [Fact]
    public async Task WebLoginFuncionario_Operacional_GravaSessaoDeFuncionarioEDeGestao()
    {
        var (resultado, sessao) = await EntrarComoFuncionarioAsync("OPERACIONAL");

        var redirecionamento = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("MeuPonto", redirecionamento.ControllerName);

        // Área do funcionário (bater ponto).
        Assert.Equal("token-da-rita", sessao.GetString("FuncionarioAuthToken"));
        Assert.Equal("FUNCIONARIO", sessao.GetString("TipoConta"));

        // Área de gestão: mesmo token, nível OPERACIONAL.
        Assert.Equal("token-da-rita", sessao.GetString("AuthToken"));
        Assert.Equal("OPERACIONAL", sessao.GetString("UsuarioNivel"));
    }

    [Fact]
    public async Task WebLoginFuncionario_Comum_NaoGanhaAcessoDeGestao()
    {
        var (_, sessao) = await EntrarComoFuncionarioAsync("FUNCIONARIO");

        Assert.Equal("token-da-rita", sessao.GetString("FuncionarioAuthToken"));
        Assert.Null(sessao.GetString("AuthToken"));
        Assert.Null(sessao.GetString("UsuarioNivel"));
    }

    // ---------- Web: só o ADMIN escolhe o nível ----------

    private static (WebFuncionariosController Controller, ServicoFuncionarioFalso Servico) CriarControllerDeFuncionarios(
        string nivel)
    {
        var contextoHttp = new DefaultHttpContext { Session = new SessaoFalsa() };
        contextoHttp.Session.SetString("AuthToken", "token-de-teste");
        contextoHttp.Session.SetString("UsuarioNivel", nivel);

        var servico = new ServicoFuncionarioFalso();

        var controller = new WebFuncionariosController(servico, null!)
        {
            ControllerContext = new ControllerContext { HttpContext = contextoHttp },
            TempData = new TempDataDictionary(contextoHttp, new ProvedorTempDataFalso())
        };

        return (controller, servico);
    }

    private static FuncionarioCadastroInputModel CadastroWeb(string nivel) => new()
    {
        Nome = "Novo",
        Sobrenome = "Colaborador",
        EmailCorporativo = "novo@empresa.com",
        Senha = "Senha@12345",
        CPF = "12345678901",
        DataAdmissao = new DateTime(2026, 5, 1),
        NivelAcesso = nivel
    };

    [Fact]
    public async Task WebCadastrar_Operacional_TentandoCriarOperacional_SaiComoFuncionario()
    {
        var (controller, servico) = CriarControllerDeFuncionarios("OPERACIONAL");

        await controller.Cadastrar(CadastroWeb("OPERACIONAL"), CancellationToken.None);

        Assert.NotNull(servico.UltimoCadastro);
        Assert.Equal("FUNCIONARIO", servico.UltimoCadastro!.NivelAcesso);
    }

    [Fact]
    public async Task WebCadastrar_Admin_PodeCriarOperacional()
    {
        var (controller, servico) = CriarControllerDeFuncionarios("ADMIN");

        await controller.Cadastrar(CadastroWeb("OPERACIONAL"), CancellationToken.None);

        Assert.Equal("OPERACIONAL", servico.UltimoCadastro!.NivelAcesso);
    }

    [Fact]
    public async Task WebEditar_Operacional_NaoEnviaMudancaDeNivel()
    {
        var (controller, servico) = CriarControllerDeFuncionarios("OPERACIONAL");

        await controller.Editar(
            1,
            new FuncionarioEdicaoInputModel
            {
                Nome = "Nome",
                Sobrenome = "Teste",
                EmailCorporativo = "maria@empresa.com",
                CPF = "12345678901",
                DataAdmissao = new DateTime(2026, 1, 1),
                NivelAcesso = "OPERACIONAL"
            },
            CancellationToken.None);

        Assert.NotNull(servico.UltimaEdicao);
        Assert.Null(servico.UltimaEdicao!.NivelAcesso);
    }
}
