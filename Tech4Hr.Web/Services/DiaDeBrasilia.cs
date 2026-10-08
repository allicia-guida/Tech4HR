namespace Tech4Hr.Web.Services;

/// <summary>
/// O dia de hoje no fuso de Brasília, para as telas que precisam de uma data de referência
/// (o painel). Não usa o relógio do servidor Web: entre 21h e meia-noite em Brasília um
/// servidor em UTC já está no dia seguinte.
/// </summary>
public static class DiaDeBrasilia
{
    private static readonly TimeZoneInfo Fuso = Localizar();

    public static DateOnly Hoje(TimeProvider relogio)
    {
        ArgumentNullException.ThrowIfNull(relogio);

        var emBrasilia = TimeZoneInfo.ConvertTime(relogio.GetUtcNow(), Fuso);

        return DateOnly.FromDateTime(emBrasilia.DateTime);
    }

    private static TimeZoneInfo Localizar()
    {
        foreach (var id in new[] { "America/Sao_Paulo", "E. South America Standard Time" })
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

        // O Brasil não tem horário de verão desde 2019: sem o banco de fusos, vale UTC-3 fixo.
        return TimeZoneInfo.CreateCustomTimeZone(
            "BRT", TimeSpan.FromHours(-3), "Brasília", "Brasília");
    }
}
