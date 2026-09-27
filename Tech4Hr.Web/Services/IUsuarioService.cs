using Tech4Hr.Web.Models;

namespace Tech4Hr.Web.Services;

public interface IUsuarioService
{
    Task<IReadOnlyList<UsuarioApiResponse>> ListarAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task<UsuarioApiResponse?> BuscarPorIdAsync(
        int id,
        string token,
        CancellationToken cancellationToken = default);

    Task<UsuarioApiResponse> CriarAsync(
        UsuarioCadastroInputModel model,
        string token,
        CancellationToken cancellationToken = default);

    Task<UsuarioApiResponse?> AtualizarAsync(
        int id,
        UsuarioEdicaoInputModel model,
        string token,
        CancellationToken cancellationToken = default);

    Task<UsuarioApiResponse?> AlterarStatusAsync(
        int id,
        bool ativo,
        string token,
        CancellationToken cancellationToken = default);
}
