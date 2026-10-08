namespace Tech4Hr.API.Services;

/// <summary>
/// Fonte única de data e hora do ponto. O horário de negócio é sempre o de
/// Brasília, independente do fuso do servidor onde a API roda ou do aparelho
/// de quem bate o ponto. A procedure sp_RegistrarPonto usa o mesmo fuso.
/// </summary>
public sealed class RelogioBrasil
{
    // O Linux usa o nome IANA e o Windows usa o nome próprio. Tenta os dois.
    private static readonly string[] IdsDoFuso =
    {
        "America/Sao_Paulo",
        "E. South America Standard Time"
    };

    private readonly TimeProvider _timeProvider;

    public RelogioBrasil(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        Fuso = LocalizarFuso();
    }

    public TimeZoneInfo Fuso { get; }

    /// <summary>Nome IANA entregue aos clientes (navegador e app).</summary>
    public string NomeDoFuso => "America/Sao_Paulo";

    /// <summary>Instante atual, já com o deslocamento de Brasília.</summary>
    public DateTimeOffset Agora =>
        TimeZoneInfo.ConvertTime(_timeProvider.GetUtcNow(), Fuso);

    /// <summary>Dia corrente em Brasília. É esse dia que fecha o espelho do ponto.</summary>
    public DateOnly Hoje => DateOnly.FromDateTime(Agora.DateTime);

    /// <summary>Converte qualquer instante para o horário de Brasília.</summary>
    public DateTimeOffset ParaBrasil(DateTimeOffset instante) =>
        TimeZoneInfo.ConvertTime(instante, Fuso);

    private static TimeZoneInfo LocalizarFuso()
    {
        foreach (var id in IdsDoFuso)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        throw new InvalidOperationException(
            "O fuso horário de Brasília não foi encontrado neste sistema operacional.");
    }
}
