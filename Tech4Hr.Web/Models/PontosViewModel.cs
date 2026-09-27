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
    public DateTime DataAtual { get; set; } = DateTime.Now;
    public PontoApiResponse? PontoHoje { get; set; }
}

public sealed class HistoricoMeuPontoViewModel
{
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public IReadOnlyList<PontoApiResponse> Registros { get; set; } = Array.Empty<PontoApiResponse>();
}

