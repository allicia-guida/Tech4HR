using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Tech4Hr.API.Controllers;
using Tech4Hr.API.Data;
using Tech4Hr.API.DTOs;
using Tech4Hr.API.Models;

namespace Tech4Hr.Tests;

public class AuthAuthorizationTests
{
    private static Tech4HrDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<Tech4HrDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new Tech4HrDbContext(options);
    }

    [Fact]
    public async Task Login_Usuario_Valido_GeraTokenComTipoConta()
    {
        await using var context = CreateContext();

        var usuario = new Usuario
        {
            Nome = "Admin",
            Sobrenome = "Teste",
            Email = "admin@teste.com",
            SenhaHash = new PasswordHasher<Usuario>().HashPassword(null!, "Senha@123"),
            NivelUsuario = "ADMIN",
            Ativo = true
        };

        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "Tech4HrTests",
                ["Jwt:Audience"] = "Tech4HrAudience",
                ["Jwt:Key"] = Convert.ToBase64String(Encoding.UTF8.GetBytes("12345678901234567890123456789012")),
                ["Jwt:ExpirationMinutes"] = "60"
            })
            .Build();

        var controller = new AuthController(context, config);
        var result = await controller.Login(new LoginUsuarioDto
        {
            Email = "admin@teste.com",
            Senha = "Senha@123"
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var token = ok.Value!
            .GetType()
            .GetProperty("token")!
            .GetValue(ok.Value) as string;

        Assert.False(string.IsNullOrWhiteSpace(token));
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        Assert.Contains(jwt.Claims, c => c.Type == "tipo_conta" && c.Value == "USUARIO");
        Assert.Contains(jwt.Claims, c => c.Type == "role" && c.Value == "ADMIN");
    }

    [Fact]
    public async Task Login_Funcionario_Valido_GeraTokenComTipoContaFuncionario()
    {
        await using var context = CreateContext();

        var funcionario = new Funcionario
        {
            Nome = "Maria",
            Sobrenome = "Silva",
            EmailCorporativo = "maria@empresa.com",
            CPF = "12345678909",
            SenhaHash = new PasswordHasher<Funcionario>().HashPassword(null!, "Senha@123"),
            DataAdmissao = DateTime.Today,
            Ativo = true
        };

        context.Funcionarios.Add(funcionario);
        await context.SaveChangesAsync();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "Tech4HrTests",
                ["Jwt:Audience"] = "Tech4HrAudience",
                ["Jwt:Key"] = Convert.ToBase64String(Encoding.UTF8.GetBytes("12345678901234567890123456789012")),
                ["Jwt:ExpirationMinutes"] = "60"
            })
            .Build();

        var controller = new AuthController(context, config);
        var result = await controller.LoginFuncionario(new LoginFuncionarioDto
        {
            EmailCorporativo = "maria@empresa.com",
            Senha = "Senha@123"
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var token = ok.Value!
            .GetType()
            .GetProperty("token")!
            .GetValue(ok.Value) as string;

        Assert.False(string.IsNullOrWhiteSpace(token));
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        Assert.Contains(jwt.Claims, c => c.Type == "tipo_conta" && c.Value == "FUNCIONARIO");
        Assert.Contains(jwt.Claims, c => c.Type == "role" && c.Value == "FUNCIONARIO");
    }
}
