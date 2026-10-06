using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tech4Hr.API.Controllers;
using Tech4Hr.API.Data;
using Tech4Hr.API.DTOs;
using Tech4Hr.API.Models;
using Tech4Hr.API.Services;
using Xunit;

namespace Tech4Hr.Tests;

public class PontoHorarioTests
{
    // Relógio com hora fixa, para o teste não depender de quando ele roda.
    private sealed class RelogioFixo : TimeProvider
    {
        private readonly DateTimeOffset _utc;

        public RelogioFixo(DateTimeOffset utc) => _utc = utc;

        public override DateTimeOffset GetUtcNow() => _utc;
    }

    private static RelogioBrasil CriarRelogio(string instanteUtc) =>
        new(new RelogioFixo(DateTimeOffset.Parse(instanteUtc)));

    private static Tech4HrDbContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<Tech4HrDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new Tech4HrDbContext(options);
    }

    private static PontosController CriarController(
        Tech4HrDbContext contexto,
        RelogioBrasil relogio,
        int? idFuncionario)
    {
        var identidade = idFuncionario.HasValue
            ? new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, idFuncionario.Value.ToString()),
                    new Claim("tipo_conta", "FUNCIONARIO")
                },
                "teste")
            : new ClaimsIdentity();

        return new PontosController(contexto, relogio)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identidade)
                }
            }
        };
    }

    [Fact]
    public void Relogio_22h30EmBrasilia_AindaEhODiaAnterior()
    {
        // 01:30 UTC de 06/10 equivale a 22:30 de 05/10 em Brasília.
        var relogio = CriarRelogio("2026-10-06T01:30:00Z");

        Assert.Equal(new DateOnly(2026, 10, 5), relogio.Hoje);
        Assert.Equal(22, relogio.Agora.Hour);
        Assert.Equal(TimeSpan.FromHours(-3), relogio.Agora.Offset);
    }

    [Fact]
    public void Relogio_MeiaNoiteEmBrasilia_ViraODia()
    {
        // 03:00 UTC é exatamente 00:00 em Brasília.
        var relogio = CriarRelogio("2026-10-06T03:00:00Z");

        Assert.Equal(new DateOnly(2026, 10, 6), relogio.Hoje);
        Assert.Equal(0, relogio.Agora.Hour);
    }

    [Fact]
    public void Relogio_ParaBrasil_PreservaOInstante()
    {
        var relogio = CriarRelogio("2026-10-06T12:00:00Z");
        var utc = DateTimeOffset.Parse("2026-10-06T15:45:00Z");

        var local = relogio.ParaBrasil(utc);

        Assert.Equal(utc, local);
        Assert.Equal(12, local.Hour);
        Assert.Equal(45, local.Minute);
    }

    [Fact]
    public void ProximoTipo_SemPonto_ComecaPelaEntrada()
    {
        Assert.Equal(JornadaPonto.Entrada, JornadaPonto.ProximoTipo(null));
        Assert.Equal(JornadaPonto.Entrada, JornadaPonto.ProximoTipo(new Ponto()));
    }

    [Fact]
    public void ProximoTipo_SegueAOrdemDaJornada()
    {
        var hoje = new DateTime(2026, 10, 5);
        var ponto = new Ponto { Entrada = hoje.AddHours(8) };
        Assert.Equal(JornadaPonto.SaidaAlmoco, JornadaPonto.ProximoTipo(ponto));

        ponto.SaidaAlmoco = hoje.AddHours(12);
        Assert.Equal(JornadaPonto.EntradaAlmoco, JornadaPonto.ProximoTipo(ponto));

        ponto.EntradaAlmoco = hoje.AddHours(13);
        Assert.Equal(JornadaPonto.Saida, JornadaPonto.ProximoTipo(ponto));

        ponto.Saida = hoje.AddHours(17);
        Assert.Null(JornadaPonto.ProximoTipo(ponto));
    }

    [Theory]
    [InlineData(50001)]
    [InlineData(50002)]
    [InlineData(50003)]
    public void MensagemDeRegra_ErrosDaProcedure_TemMensagem(int numero)
    {
        Assert.False(string.IsNullOrWhiteSpace(JornadaPonto.MensagemDeRegra(numero)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2627)]
    [InlineData(50010)]
    public void MensagemDeRegra_OutrosErros_NaoTemMensagem(int numero)
    {
        Assert.Null(JornadaPonto.MensagemDeRegra(numero));
    }

    [Fact]
    public async Task Hoje_UsaODiaDeBrasilia_NaoODiaDoServidor()
    {
        await using var contexto = CriarContexto();

        // Em UTC já é 06/10, mas em Brasília ainda é 05/10 às 22:30.
        var relogio = CriarRelogio("2026-10-06T01:30:00Z");

        contexto.Pontos.AddRange(
            new Ponto
            {
                IdFuncionario = 1,
                DataPonto = new DateTime(2026, 10, 5),
                Entrada = new DateTime(2026, 10, 5, 8, 0, 0)
            },
            new Ponto
            {
                IdFuncionario = 1,
                DataPonto = new DateTime(2026, 10, 6),
                Entrada = new DateTime(2026, 10, 6, 8, 0, 0)
            });

        await contexto.SaveChangesAsync();

        var controller = CriarController(contexto, relogio, idFuncionario: 1);

        var resultado = await controller.Hoje();

        var ok = Assert.IsType<OkObjectResult>(resultado);
        var resposta = Assert.IsType<PontoHojeResponse>(ok.Value);

        Assert.Equal(new DateOnly(2026, 10, 5), resposta.DataReferencia);
        Assert.Equal("America/Sao_Paulo", resposta.FusoHorario);
        Assert.Equal(TimeSpan.FromHours(-3), resposta.Agora.Offset);
        Assert.NotNull(resposta.Ponto);
        Assert.Equal(new DateTime(2026, 10, 5), resposta.Ponto!.DataPonto);
        Assert.Equal(JornadaPonto.SaidaAlmoco, resposta.ProximoTipoRegistro);
    }

    [Fact]
    public async Task Hoje_SemBatidasNoDia_IndicaEntradaComoProxima()
    {
        await using var contexto = CriarContexto();
        var relogio = CriarRelogio("2026-10-05T15:00:00Z");

        var controller = CriarController(contexto, relogio, idFuncionario: 7);

        var resultado = await controller.Hoje();

        var ok = Assert.IsType<OkObjectResult>(resultado);
        var resposta = Assert.IsType<PontoHojeResponse>(ok.Value);

        Assert.Null(resposta.Ponto);
        Assert.Equal(JornadaPonto.Entrada, resposta.ProximoTipoRegistro);
    }

    [Fact]
    public async Task Hoje_JornadaCompleta_NaoOfereceProximaBatida()
    {
        await using var contexto = CriarContexto();
        var relogio = CriarRelogio("2026-10-05T21:00:00Z");
        var dia = new DateTime(2026, 10, 5);

        contexto.Pontos.Add(new Ponto
        {
            IdFuncionario = 3,
            DataPonto = dia,
            Entrada = dia.AddHours(8),
            SaidaAlmoco = dia.AddHours(12),
            EntradaAlmoco = dia.AddHours(13),
            Saida = dia.AddHours(17)
        });

        await contexto.SaveChangesAsync();

        var controller = CriarController(contexto, relogio, idFuncionario: 3);

        var ok = Assert.IsType<OkObjectResult>(await controller.Hoje());
        var resposta = Assert.IsType<PontoHojeResponse>(ok.Value);

        Assert.Null(resposta.ProximoTipoRegistro);
    }

    [Fact]
    public async Task Hoje_SemIdentificacaoNoToken_RetornaNaoAutorizado()
    {
        await using var contexto = CriarContexto();
        var relogio = CriarRelogio("2026-10-05T15:00:00Z");

        var controller = CriarController(contexto, relogio, idFuncionario: null);

        Assert.IsType<UnauthorizedResult>(await controller.Hoje());
    }
}
