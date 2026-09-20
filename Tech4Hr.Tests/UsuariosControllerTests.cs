using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tech4Hr.API.Controllers;
using Tech4Hr.API.Data;
using Tech4Hr.API.DTOs;
using Tech4Hr.API.Models;
using Xunit;

namespace Tech4Hr.Tests;

public class UsuariosControllerTests
{
    private static Tech4HrDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<Tech4HrDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new Tech4HrDbContext(options);
    }

    [Fact]
    public async Task AlterarStatus_UltimoAdminAtivo_DeveRetornarConflict()
    {
        await using var context = CreateContext();

        context.Usuarios.Add(new Usuario
        {
            Nome = "Admin",
            Sobrenome = "Principal",
            Email = "admin@teste.com",
            SenhaHash = "hash",
            NivelUsuario = "ADMIN",
            Ativo = true
        });

        await context.SaveChangesAsync();

        var controller = new UsuariosController(context);

        var result = await controller.AlterarStatus(1, new AlterarUsuarioStatusDto { Ativo = false });

        var conflict = Assert.IsType<ConflictObjectResult>(result);
        Assert.NotNull(conflict.Value);
    }

    [Fact]
    public async Task Editar_RebaixarUltimoAdminAtivo_DeveRetornarConflict()
    {
        await using var context = CreateContext();

        context.Usuarios.Add(new Usuario
        {
            Nome = "Admin",
            Sobrenome = "Principal",
            Email = "admin@teste.com",
            SenhaHash = "hash",
            NivelUsuario = "ADMIN",
            Ativo = true
        });

        await context.SaveChangesAsync();

        var controller = new UsuariosController(context);

        var result = await controller.Editar(1, new EditarUsuarioDto
        {
            Nome = "Admin",
            Sobrenome = "Principal",
            Email = "admin@teste.com",
            NivelUsuario = "OPERACIONAL"
        });

        var conflict = Assert.IsType<ConflictObjectResult>(result);
        Assert.NotNull(conflict.Value);
    }
}
