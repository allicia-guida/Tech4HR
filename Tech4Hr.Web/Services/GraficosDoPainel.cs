using Tech4Hr.Web.Models;

namespace Tech4Hr.Web.Services;

/// <summary>
/// Calcula os dados dos gráficos do painel a partir das listas que a API já devolve.
/// Só faz conta: não conhece tela, sessão nem relógio. O dia de "hoje" chega pronto.
/// </summary>
public static class GraficosDoPainel
{
    public const int DiasDoPeriodo = 14;

    /// <summary>Ativos e inativos. O inativo vai em cinza para não competir com o que importa.</summary>
    public static GraficoBarraEmpilhada Equipe(IReadOnlyList<FuncionarioApiResponse> funcionarios)
    {
        var ativos = funcionarios.Count(f => f.Ativo);

        return new GraficoBarraEmpilhada
        {
            Id = "grafico-equipe",
            Titulo = "Equipe",
            Subtitulo = "funcionários cadastrados",
            Segmentos = new[]
            {
                new GraficoSegmento("Ativos", ativos, "1"),
                new GraficoSegmento("Inativos", funcionarios.Count - ativos, "neutro")
            }
        };
    }

    /// <summary>
    /// Onde cada funcionário ativo está no dia de hoje. Quem não tem ponto aberto entra em
    /// "Sem registro". Funcionário inativo não conta, mesmo que tenha ponto no dia.
    /// </summary>
    public static GraficoBarraEmpilhada Hoje(
        IReadOnlyList<FuncionarioApiResponse> funcionarios,
        IReadOnlyList<PontoApiResponse> pontos,
        DateOnly hoje)
    {
        var ativos = funcionarios.Where(f => f.Ativo).Select(f => f.IdFuncionario).ToHashSet();

        var doDia = pontos
            .Where(p => DateOnly.FromDateTime(p.DataPonto) == hoje && ativos.Contains(p.IdFuncionario))
            .GroupBy(p => p.IdFuncionario)
            .Select(grupo => grupo.First())
            .ToList();

        var encerrada = doDia.Count(p => p.Saida.HasValue);

        var emAlmoco = doDia.Count(p =>
            !p.Saida.HasValue && p.SaidaAlmoco.HasValue && !p.EntradaAlmoco.HasValue);

        var trabalhando = doDia.Count(p =>
            !p.Saida.HasValue
            && p.Entrada.HasValue
            && !(p.SaidaAlmoco.HasValue && !p.EntradaAlmoco.HasValue));

        var semRegistro = Math.Max(0, ativos.Count - encerrada - emAlmoco - trabalhando);

        return new GraficoBarraEmpilhada
        {
            Id = "grafico-hoje",
            Titulo = "Situação de hoje",
            Subtitulo = "funcionários ativos",
            Segmentos = new[]
            {
                new GraficoSegmento("Trabalhando", trabalhando, "1"),
                new GraficoSegmento("Em almoço", emAlmoco, "2"),
                new GraficoSegmento("Jornada encerrada", encerrada, "3"),
                new GraficoSegmento("Sem registro", semRegistro, "neutro")
            }
        };
    }

    /// <summary>
    /// Expedientes iniciados em cada um dos últimos dias, separados em completos (com saída) e
    /// em aberto (sem saída). Em dias passados, "em aberto" costuma ser batida esquecida.
    /// </summary>
    public static GraficoColunas Dias(
        IReadOnlyList<PontoApiResponse> pontos,
        DateOnly hoje,
        int quantidade = DiasDoPeriodo)
    {
        var inicio = hoje.AddDays(-(quantidade - 1));
        var dias = new List<GraficoDia>(quantidade);

        for (var dia = inicio; dia <= hoje; dia = dia.AddDays(1))
        {
            var iniciados = pontos
                .Where(p => DateOnly.FromDateTime(p.DataPonto) == dia && p.Entrada.HasValue)
                .ToList();

            dias.Add(new GraficoDia(
                dia,
                iniciados.Count(p => p.Saida.HasValue),
                iniciados.Count(p => !p.Saida.HasValue)));
        }

        return new GraficoColunas
        {
            Id = "grafico-dias",
            Titulo = $"Expedientes dos últimos {quantidade} dias",
            Subtitulo = "funcionários com entrada registrada em cada dia",
            Dias = dias,
            Maximo = TopoDoEixo(dias.Max(d => d.Total)),
            Hoje = hoje
        };
    }

    /// <summary>Menor múltiplo de 4 que cobre o maior valor (mínimo 4), para os números do eixo serem inteiros.</summary>
    public static int TopoDoEixo(int maior) => Math.Max(4, (maior + 3) / 4 * 4);
}
