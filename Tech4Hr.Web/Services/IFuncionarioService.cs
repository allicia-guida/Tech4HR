using Tech4Hr.Web.Models;

namespace Tech4Hr.Web.Services;

public interface IFuncionarioService
{
    Task<IReadOnlyList<FuncionarioApiResponse>> ListarAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task<FuncionarioApiResponse?> BuscarPorIdAsync(
        int id,
        string token,
        CancellationToken cancellationToken = default);

    Task<FuncionarioApiResponse> CriarAsync(
        FuncionarioCadastroInputModel model,
        string token,
        CancellationToken cancellationToken = default);

    Task<FuncionarioApiResponse?> AtualizarAsync(
        int id,
        FuncionarioEdicaoInputModel model,
        string token,
        CancellationToken cancellationToken = default);

    Task<FuncionarioApiResponse?> AlterarStatusAsync(
        int id,
        bool ativo,
        string token,
        CancellationToken cancellationToken = default);
}

