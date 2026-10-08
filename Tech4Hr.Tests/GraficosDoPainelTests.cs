using Tech4Hr.Web.Models;
using Tech4Hr.Web.Services;
using Xunit;

namespace Tech4Hr.Tests;

// Contas dos gráficos do painel e o dia de Brasília que serve de referência.
public class GraficosDoPainelTests
{
    private static readonly DateOnly Hoje = new(2026, 10, 7);

    private sealed class RelogioFixo(DateTimeOffset agora) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => agora;
    }

    private static FuncionarioApiResponse Func(int id, bool ativo = true, string nivel = "FUNCIONARIO") =>
        new() { IdFuncionario = id, Ativo = ativo, NivelAcesso = nivel };

    private static PontoApiResponse Ponto(
        int idFuncionario,
        DateOnly dia,
        int? entrada = null,
        int? saidaAlmoco = null,
        int? entradaAlmoco = null,
        int? saida = null)
    {
        DateTime? Hora(int? h) => h.HasValue ? dia.ToDateTime(new TimeOnly(h.Value, 0)) : null;

        return new PontoApiResponse
        {
            IdFuncionario = idFuncionario,
            DataPonto = dia.ToDateTime(TimeOnly.MinValue),
            Entrada = Hora(entrada),
            SaidaAlmoco = Hora(saidaAlmoco),
            EntradaAlmoco = Hora(entradaAlmoco),
            Saida = Hora(saida)
        };
    }

    private static int Valor(GraficoBarraEmpilhada grafico, string rotulo) =>
        grafico.Segmentos.Single(s => s.Rotulo == rotulo).Valor;

    // ---------- Equipe ----------

    [Fact]
    public void Equipe_SeparaAtivosDeInativos()
    {
        var grafico = GraficosDoPainel.Equipe(new[] { Func(1), Func(2), Func(3, ativo: false) });

        Assert.Equal(2, Valor(grafico, "Ativos"));
        Assert.Equal(1, Valor(grafico, "Inativos"));
        Assert.Equal(3, grafico.Total);
    }

    [Fact]
    public void Equipe_SemFuncionarios_FicaVaziaSemQuebrar()
    {
        var grafico = GraficosDoPainel.Equipe(Array.Empty<FuncionarioApiResponse>());

        Assert.Equal(0, grafico.Total);
        Assert.Contains("sem dados", grafico.Resumo);
    }

    // ---------- Situação de hoje ----------

    [Fact]
    public void Hoje_ClassificaCadaFuncionarioAtivoPeloPontoDoDia()
    {
        var funcionarios = Enumerable.Range(1, 5).Select(i => Func(i)).ToList();

        var pontos = new[]
        {
            Ponto(1, Hoje, entrada: 8),                                                // trabalhando
            Ponto(2, Hoje, entrada: 8, saidaAlmoco: 12),                               // em almoço
            Ponto(3, Hoje, entrada: 8, saidaAlmoco: 12, entradaAlmoco: 13, saida: 17), // encerrada
            Ponto(4, Hoje, entrada: 8, saidaAlmoco: 12, entradaAlmoco: 13)             // voltou do almoço, trabalhando
            // 5: sem ponto
        };

        var grafico = GraficosDoPainel.Hoje(funcionarios, pontos, Hoje);

        Assert.Equal(2, Valor(grafico, "Trabalhando"));
        Assert.Equal(1, Valor(grafico, "Em almoço"));
        Assert.Equal(1, Valor(grafico, "Jornada encerrada"));
        Assert.Equal(1, Valor(grafico, "Sem registro"));
    }

    [Fact]
    public void Hoje_ASomaDasCategoriasSempreFechaComOTotalDeAtivos()
    {
        var funcionarios = new[] { Func(1), Func(2), Func(3), Func(4, ativo: false) };
        var pontos = new[] { Ponto(1, Hoje, entrada: 8), Ponto(4, Hoje, entrada: 8) };

        var grafico = GraficosDoPainel.Hoje(funcionarios, pontos, Hoje);

        Assert.Equal(3, grafico.Total);
    }

    [Fact]
    public void Hoje_IgnoraFuncionarioInativoEPontoDeOutroDia()
    {
        var funcionarios = new[] { Func(1), Func(2, ativo: false) };

        var pontos = new[]
        {
            Ponto(2, Hoje, entrada: 8),                    // inativo: não conta
            Ponto(1, Hoje.AddDays(-1), entrada: 8, saida: 17) // ontem: não é de hoje
        };

        var grafico = GraficosDoPainel.Hoje(funcionarios, pontos, Hoje);

        Assert.Equal(1, Valor(grafico, "Sem registro"));
        Assert.Equal(0, Valor(grafico, "Trabalhando"));
        Assert.Equal(0, Valor(grafico, "Jornada encerrada"));
    }

    // ---------- Expedientes por dia ----------

    [Fact]
    public void Dias_TrazOsUltimosQuatorzeDiasTerminandoEmHoje()
    {
        var grafico = GraficosDoPainel.Dias(Array.Empty<PontoApiResponse>(), Hoje);

        Assert.Equal(14, grafico.Dias.Count);
        Assert.Equal(Hoje.AddDays(-13), grafico.Dias[0].Data);
        Assert.Equal(Hoje, grafico.Dias[^1].Data);
        Assert.Equal(Hoje, grafico.Hoje);
        Assert.Equal(4, grafico.Maximo);
    }

    [Fact]
    public void Dias_SeparaJornadaCompletaDeEmAberto()
    {
        var ontem = Hoje.AddDays(-1);

        var pontos = new[]
        {
            Ponto(1, ontem, entrada: 8, saidaAlmoco: 12, entradaAlmoco: 13, saida: 17), // completa
            Ponto(2, ontem, entrada: 8, saidaAlmoco: 12),                               // esqueceu de bater
            Ponto(3, ontem, entrada: 8, saida: 17),                                     // tem saída: completa
            Ponto(4, Hoje, entrada: 9)                                                  // em aberto hoje
        };

        var grafico = GraficosDoPainel.Dias(pontos, Hoje);

        var diaOntem = grafico.Dias.Single(d => d.Data == ontem);
        Assert.Equal(2, diaOntem.Completas);
        Assert.Equal(1, diaOntem.EmAberto);

        var diaHoje = grafico.Dias.Single(d => d.Data == Hoje);
        Assert.Equal(0, diaHoje.Completas);
        Assert.Equal(1, diaHoje.EmAberto);
    }

    [Fact]
    public void Dias_IgnoraPontoSemEntradaEDiasForaDoPeriodo()
    {
        var pontos = new[]
        {
            Ponto(1, Hoje, saida: 17),                 // sem entrada: não conta como expediente iniciado
            Ponto(2, Hoje.AddDays(-14), entrada: 8),   // um dia antes do período
            Ponto(3, Hoje.AddDays(1), entrada: 8)      // amanhã
        };

        var grafico = GraficosDoPainel.Dias(pontos, Hoje);

        Assert.Equal(0, grafico.TotalCompletas + grafico.TotalEmAberto);
    }

    [Theory]
    [InlineData(0, 4)]
    [InlineData(1, 4)]
    [InlineData(4, 4)]
    [InlineData(5, 8)]
    [InlineData(8, 8)]
    [InlineData(9, 12)]
    [InlineData(17, 20)]
    public void TopoDoEixo_ArredondaParaMultiploDeQuatroComMinimoDeQuatro(int maior, int esperado)
    {
        Assert.Equal(esperado, GraficosDoPainel.TopoDoEixo(maior));
    }

    // ---------- Dia de Brasília ----------

    [Fact]
    public void DiaDeBrasilia_AsDuasDaManhaUtcAindaEOdiaAnteriorEmBrasilia()
    {
        // 07/10 23:00 em Brasília já é 08/10 02:00 em UTC.
        var relogio = new RelogioFixo(new DateTimeOffset(2026, 10, 8, 2, 0, 0, TimeSpan.Zero));

        Assert.Equal(new DateOnly(2026, 10, 7), DiaDeBrasilia.Hoje(relogio));
    }

    [Fact]
    public void DiaDeBrasilia_AMeiaNoiteDeBrasiliaViraODiaSeguinte()
    {
        var antes = new RelogioFixo(new DateTimeOffset(2026, 10, 8, 2, 59, 59, TimeSpan.Zero));
        var depois = new RelogioFixo(new DateTimeOffset(2026, 10, 8, 3, 0, 0, TimeSpan.Zero));

        Assert.Equal(new DateOnly(2026, 10, 7), DiaDeBrasilia.Hoje(antes));
        Assert.Equal(new DateOnly(2026, 10, 8), DiaDeBrasilia.Hoje(depois));
    }

    [Fact]
    public void DiaDeBrasilia_NoMeioDoDiaOsDoisRelogiosConcordam()
    {
        var relogio = new RelogioFixo(new DateTimeOffset(2026, 10, 7, 15, 0, 0, TimeSpan.Zero));

        Assert.Equal(new DateOnly(2026, 10, 7), DiaDeBrasilia.Hoje(relogio));
    }
}
