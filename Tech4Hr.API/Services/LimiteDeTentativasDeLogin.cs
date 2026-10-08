namespace Tech4Hr.API.Services;

/// <summary>
/// Limite de tentativas de login por conta (e-mail). Depois de um número de
/// senhas erradas dentro de uma janela de tempo, a conta fica bloqueada por um
/// período e responde 429, mesmo que a senha certa chegue.
///
/// O controle é por conta e não por IP porque o Web chama a API de servidor para
/// servidor: a API só enxerga o IP do servidor Web, e todos os usuários
/// dividiriam o mesmo limite. A contagem vale também para e-mails que não
/// existem, para que o bloqueio não revele quais contas existem.
///
/// Os contadores ficam na memória do processo: reiniciar a API zera tudo, e com
/// mais de uma instância cada uma conta por conta própria.
/// </summary>
public sealed class LimiteDeTentativasDeLogin
{
    private const int MaximoDeContasRastreadas = 20_000;

    private readonly TimeProvider _relogio;
    private readonly int _maxFalhas;
    private readonly TimeSpan _janela;
    private readonly TimeSpan _bloqueio;
    private readonly Dictionary<string, Registro> _contas = new();
    private readonly object _trava = new();

    public LimiteDeTentativasDeLogin(
        TimeProvider relogio,
        int maxFalhas = 5,
        TimeSpan? janela = null,
        TimeSpan? bloqueio = null)
    {
        ArgumentNullException.ThrowIfNull(relogio);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxFalhas, 1);

        _relogio = relogio;
        _maxFalhas = maxFalhas;
        _janela = janela ?? TimeSpan.FromMinutes(10);
        _bloqueio = bloqueio ?? TimeSpan.FromMinutes(10);

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(_janela, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(_bloqueio, TimeSpan.Zero);
    }

    /// <summary>Chave de uma conta: tipo (USUARIO ou FUNCIONARIO) e e-mail normalizado.</summary>
    public static string Chave(string tipoConta, string email) =>
        $"{tipoConta}:{email.Trim().ToLowerInvariant()}";

    /// <summary>Tempo que falta para a conta voltar a aceitar login, ou null se não está bloqueada.</summary>
    public TimeSpan? TempoRestanteDeBloqueio(string chave)
    {
        var agora = _relogio.GetUtcNow();

        lock (_trava)
        {
            if (!_contas.TryGetValue(chave, out var registro) || registro.BloqueadoAte is null)
            {
                return null;
            }

            if (registro.BloqueadoAte.Value <= agora)
            {
                _contas.Remove(chave);
                return null;
            }

            return registro.BloqueadoAte.Value - agora;
        }
    }

    /// <summary>Registra uma senha errada (ou e-mail desconhecido) para a conta.</summary>
    public void RegistrarFalha(string chave)
    {
        var agora = _relogio.GetUtcNow();

        lock (_trava)
        {
            if (!_contas.TryGetValue(chave, out var registro))
            {
                if (_contas.Count >= MaximoDeContasRastreadas)
                {
                    LimparExpirados(agora);

                    if (_contas.Count >= MaximoDeContasRastreadas)
                    {
                        // Memória cheia de contas ainda ativas: não rastreia mais uma.
                        return;
                    }
                }

                registro = new Registro { InicioDaJanela = agora };
                _contas[chave] = registro;
            }

            if (registro.BloqueadoAte is { } ate && ate > agora)
            {
                // Já bloqueada: tentativas durante o bloqueio não o prolongam.
                return;
            }

            if (registro.BloqueadoAte is not null || agora - registro.InicioDaJanela > _janela)
            {
                // O bloqueio acabou ou a janela venceu: recomeça a contagem.
                registro.Falhas = 0;
                registro.InicioDaJanela = agora;
                registro.BloqueadoAte = null;
            }

            registro.Falhas++;

            if (registro.Falhas >= _maxFalhas)
            {
                registro.BloqueadoAte = agora + _bloqueio;
            }
        }
    }

    /// <summary>Zera a contagem da conta depois de um login com a senha certa.</summary>
    public void RegistrarSucesso(string chave)
    {
        lock (_trava)
        {
            _contas.Remove(chave);
        }
    }

    private void LimparExpirados(DateTimeOffset agora)
    {
        var expiradas = _contas
            .Where(par =>
                (par.Value.BloqueadoAte is null || par.Value.BloqueadoAte <= agora) &&
                agora - par.Value.InicioDaJanela > _janela)
            .Select(par => par.Key)
            .ToList();

        foreach (var chave in expiradas)
        {
            _contas.Remove(chave);
        }
    }

    private sealed class Registro
    {
        public int Falhas { get; set; }
        public DateTimeOffset InicioDaJanela { get; set; }
        public DateTimeOffset? BloqueadoAte { get; set; }
    }
}
