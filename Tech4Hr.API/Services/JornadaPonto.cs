using Tech4Hr.API.Models;

namespace Tech4Hr.API.Services;

/// <summary>
/// Regras da jornada: a ordem das batidas e a tradução dos erros da procedure
/// sp_RegistrarPonto. A ordem aqui precisa ser a mesma da procedure.
/// </summary>
public static class JornadaPonto
{
    public const string Entrada = "ENTRADA";
    public const string SaidaAlmoco = "SAIDA_ALMOCO";
    public const string EntradaAlmoco = "ENTRADA_ALMOCO";
    public const string Saida = "SAIDA";

    /// <summary>
    /// Próxima batida permitida para o dia, ou null quando a jornada já foi
    /// registrada por completo.
    /// </summary>
    public static string? ProximoTipo(Ponto? pontoDoDia)
    {
        if (pontoDoDia is null || !pontoDoDia.Entrada.HasValue)
        {
            return Entrada;
        }

        if (!pontoDoDia.SaidaAlmoco.HasValue)
        {
            return SaidaAlmoco;
        }

        if (!pontoDoDia.EntradaAlmoco.HasValue)
        {
            return EntradaAlmoco;
        }

        if (!pontoDoDia.Saida.HasValue)
        {
            return Saida;
        }

        return null;
    }

    /// <summary>
    /// Mensagem em português para os erros de regra que a procedure lança com
    /// THROW (50001 a 50003). Devolve null para qualquer outro erro de SQL.
    /// </summary>
    public static string? MensagemDeRegra(int numeroErroSql) => numeroErroSql switch
    {
        50001 => "Funcionário inexistente ou inativo.",
        50002 => "A primeira marcação do dia deve ser a entrada.",
        50003 => "Marcação fora de sequência. Registre a próxima etapa da jornada.",
        _ => null
    };
}
