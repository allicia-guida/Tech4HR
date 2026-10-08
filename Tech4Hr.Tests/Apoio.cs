using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Tech4Hr.Web.Models;
using Tech4Hr.Web.Services;

namespace Tech4Hr.Tests;

// Dublês usados pelos testes das telas do Web. Ficam aqui para os arquivos de
// teste não repetirem a mesma implementação.

/// <summary>Sessão em memória, no lugar da sessão do ASP.NET.</summary>
internal sealed class SessaoFalsa : ISession
{
    private readonly Dictionary<string, byte[]> _dados = new();

    public bool IsAvailable => true;
    public string Id => "teste";
    public IEnumerable<string> Keys => _dados.Keys;
    public void Clear() => _dados.Clear();
    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public void Remove(string key) => _dados.Remove(key);
    public void Set(string key, byte[] value) => _dados[key] = value;

    public bool TryGetValue(
        string key,
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out byte[]? value) =>
        _dados.TryGetValue(key, out value);
}

/// <summary>TempData em memória.</summary>
internal sealed class ProvedorTempDataFalso : ITempDataProvider
{
    public IDictionary<string, object> LoadTempData(HttpContext context) =>
        new Dictionary<string, object>();

    public void SaveTempData(HttpContext context, IDictionary<string, object> values)
    {
    }
}

/// <summary>Serviço de funcionários que só registra o que recebeu.</summary>
internal sealed class ServicoFuncionarioFalso : IFuncionarioService
{
    public int ChamadasAlterarStatus { get; private set; }
    public FuncionarioCadastroInputModel? UltimoCadastro { get; private set; }
    public FuncionarioEdicaoInputModel? UltimaEdicao { get; private set; }

    public Task<FuncionarioApiResponse?> AlterarStatusAsync(
        int id, bool ativo, string token, CancellationToken cancellationToken = default)
    {
        ChamadasAlterarStatus++;
        return Task.FromResult<FuncionarioApiResponse?>(new FuncionarioApiResponse());
    }

    public Task<FuncionarioApiResponse> CriarAsync(
        FuncionarioCadastroInputModel model, string token,
        CancellationToken cancellationToken = default)
    {
        UltimoCadastro = model;
        return Task.FromResult(new FuncionarioApiResponse());
    }

    public Task<FuncionarioApiResponse?> AtualizarAsync(
        int id, FuncionarioEdicaoInputModel model, string token,
        CancellationToken cancellationToken = default)
    {
        UltimaEdicao = model;
        return Task.FromResult<FuncionarioApiResponse?>(new FuncionarioApiResponse());
    }

    public Task<IReadOnlyList<FuncionarioApiResponse>> ListarAsync(
        string token, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<FuncionarioApiResponse?> BuscarPorIdAsync(
        int id, string token, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
}
