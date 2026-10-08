using Tech4Hr.Web.Models;

namespace Tech4Hr.Web.Services;

public interface IPontoService
{
    Task<IReadOnlyList<PontoApiResponse>> ConsultarAsync(
        string token,
        int? idFuncionario = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        CancellationToken cancellationToken = default);

    Task<PontoRegistroResultado> RegistrarAsync(
        string token,
        string tipoRegistro,
        CancellationToken cancellationToken = default);

    Task<PontoHojeApiResponse> ObterHojeAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PontoApiResponse>> ConsultarMeusPontosAsync(
        string token,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        CancellationToken cancellationToken = default);
}
