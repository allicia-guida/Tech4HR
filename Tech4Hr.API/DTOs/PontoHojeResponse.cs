namespace Tech4Hr.API.DTOs;

/// <summary>
/// Retrato do dia de trabalho do funcionário, calculado pela API no horário de
/// Brasília. Web e app usam esta resposta em vez do relógio do próprio aparelho.
/// </summary>
public sealed class PontoHojeResponse
{
    public DateOnly DataReferencia { get; init; }

    /// <summary>Hora oficial agora, com o deslocamento de Brasília.</summary>
    public DateTimeOffset Agora { get; init; }

    public string FusoHorario { get; init; } = string.Empty;

    /// <summary>Espelho do dia, ou null se ainda não houve nenhuma batida.</summary>
    public PontoDoDiaResponse? Ponto { get; init; }

    /// <summary>Próxima batida permitida, ou null com a jornada completa.</summary>
    public string? ProximoTipoRegistro { get; init; }
}

public sealed class PontoDoDiaResponse
{
    public int IdPonto { get; init; }
    public DateTime DataPonto { get; init; }
    public DateTime? Entrada { get; init; }
    public DateTime? SaidaAlmoco { get; init; }
    public DateTime? EntradaAlmoco { get; init; }
    public DateTime? Saida { get; init; }
}
