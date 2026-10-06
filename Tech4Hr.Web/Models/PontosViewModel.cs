using System.Text.Json.Serialization;

namespace Tech4Hr.Web.Models;

public sealed class PontoFuncionarioApiResponse
{
    [JsonPropertyName("idFuncionario")]
    public int IdFuncionario { get; set; }

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("sobrenome")]
    public string Sobrenome { get; set; } = string.Empty;

    public string NomeCompleto => $"{Nome} {Sobrenome}".Trim();
}

public sealed class PontoApiResponse
{
    [JsonPropertyName("idPonto")]
    public int IdPonto { get; set; }

    [JsonPropertyName("idFuncionario")]
    public int IdFuncionario { get; set; }

    [JsonPropertyName("dataPonto")]
    public DateTime DataPonto { get; set; }

    [JsonPropertyName("entrada")]
    public DateTime? Entrada { get; set; }

    [JsonPropertyName("saidaAlmoco")]
    public DateTime? SaidaAlmoco { get; set; }

    [JsonPropertyName("entradaAlmoco")]
    public DateTime? EntradaAlmoco { get; set; }

    [JsonPropertyName("saida")]
    public DateTime? Saida { get; set; }

    [JsonPropertyName("funcionario")]
    public PontoFuncionarioApiResponse Funcionario { get; set; } = new();
}

public sealed class PontoRegistroResultado
{
    [JsonPropertyName("idPonto")]
    public int IdPonto { get; set; }

    [JsonPropertyName("tipoRegistro")]
    public string TipoRegistro { get; set; } = string.Empty;

    [JsonPropertyName("dataHora")]
    public DateTimeOffset? DataHora { get; set; }

    [JsonPropertyName("mensagem")]
    public string Mensagem { get; set; } = string.Empty;
}

/// <summary>
/// Resposta de GET api/pontos/hoje: o dia e a hora oficiais vêm da API, no
/// horário de Brasília, e não do relógio do servidor Web nem do aparelho.
/// </summary>
public sealed class PontoHojeApiResponse
{
    [JsonPropertyName("dataReferencia")]
    public DateOnly DataReferencia { get; set; }

    [JsonPropertyName("agora")]
    public DateTimeOffset Agora { get; set; }

    [JsonPropertyName("fusoHorario")]
    public string FusoHorario { get; set; } = "America/Sao_Paulo";

    [JsonPropertyName("ponto")]
    public PontoApiResponse? Ponto { get; set; }

    [JsonPropertyName("proximoTipoRegistro")]
    public string? ProximoTipoRegistro { get; set; }
}

public sealed class PontosConsultaViewModel
{
    public int? IdFuncionario { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public IReadOnlyList<FuncionarioApiResponse> Funcionarios { get; set; } = Array.Empty<FuncionarioApiResponse>();
    public IReadOnlyList<PontoApiResponse> Registros { get; set; } = Array.Empty<PontoApiResponse>();
}

public sealed class FuncionarioDetalhesViewModel
{
    public FuncionarioApiResponse Funcionario { get; set; } = new();
    public IReadOnlyList<PontoApiResponse> Pontos { get; set; } = Array.Empty<PontoApiResponse>();
}

public sealed class MeuPontoViewModel
{
    public string NomeFuncionario { get; set; } = string.Empty;
    public IReadOnlyList<PontoApiResponse> Registros { get; set; } = Array.Empty<PontoApiResponse>();
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public string? UltimoRegistro { get; set; }
    public bool PodeRegistrarPonto { get; set; } = true;
    public string ProximoTipoRegistro { get; set; } = "ENTRADA";
    public DateTime DataAtual { get; set; }

    /// <summary>Hora oficial da API em milissegundos Unix. Zero quando a API não respondeu.</summary>
    public long ServidorEpochMs { get; set; }

    public string FusoHorario { get; set; } = "America/Sao_Paulo";
    public PontoApiResponse? PontoHoje { get; set; }
}

public sealed class HistoricoMeuPontoViewModel
{
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public IReadOnlyList<PontoApiResponse> Registros { get; set; } = Array.Empty<PontoApiResponse>();
}

