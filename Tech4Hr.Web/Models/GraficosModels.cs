namespace Tech4Hr.Web.Models;

/// <summary>
/// Um trecho de uma barra empilhada. <see cref="Serie"/> escolhe a cor: "1", "2" e "3"
/// são as séries da paleta de gráficos e "neutro" é o cinza de quem não conta como destaque.
/// </summary>
public sealed record GraficoSegmento(string Rotulo, int Valor, string Serie);

public sealed class GraficoBarraEmpilhada
{
    public required string Id { get; init; }

    public required string Titulo { get; init; }

    public string? Subtitulo { get; init; }

    public required IReadOnlyList<GraficoSegmento> Segmentos { get; init; }

    public int Total => Segmentos.Sum(s => s.Valor);

    /// <summary>Frase única que um leitor de tela lê no lugar do desenho.</summary>
    public string Resumo =>
        Total == 0
            ? $"{Titulo}: sem dados."
            : $"{Titulo}: " + string.Join(", ", Segmentos.Select(s => $"{s.Rotulo} {s.Valor}"));
}

public sealed record GraficoDia(DateOnly Data, int Completas, int EmAberto)
{
    public int Total => Completas + EmAberto;
}

public sealed class GraficoColunas
{
    public required string Id { get; init; }

    public required string Titulo { get; init; }

    public string? Subtitulo { get; init; }

    public required IReadOnlyList<GraficoDia> Dias { get; init; }

    /// <summary>Topo do eixo vertical, sempre par para o meio da escala ser um número inteiro.</summary>
    public required int Maximo { get; init; }

    public DateOnly? Hoje { get; init; }

    public int TotalCompletas => Dias.Sum(d => d.Completas);

    public int TotalEmAberto => Dias.Sum(d => d.EmAberto);

    public string Resumo =>
        $"{Titulo}: {TotalCompletas} jornadas completas e {TotalEmAberto} em aberto no período.";
}

public sealed class PainelDeGraficos
{
    public GraficoBarraEmpilhada? Hoje { get; init; }

    public GraficoBarraEmpilhada? Equipe { get; init; }

    public GraficoColunas? Dias { get; init; }

    /// <summary>Preenchido quando os registros de ponto não puderam ser carregados.</summary>
    public string? Aviso { get; init; }
}
