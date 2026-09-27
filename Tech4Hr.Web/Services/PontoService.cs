using System.Net.Http.Headers;
using System.Net.Http.Json;
using Tech4Hr.Web.Models;

namespace Tech4Hr.Web.Services;

public class PontoService : IPontoService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public PontoService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IReadOnlyList<PontoApiResponse>> ConsultarAsync(
        string token,
        int? idFuncionario = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Tech4HrApi");

        var queryParameters = new List<string>();

        if (idFuncionario.HasValue)
        {
            queryParameters.Add($"idFuncionario={idFuncionario.Value}");
        }

        if (dataInicio.HasValue)
        {
            queryParameters.Add($"dataInicio={Uri.EscapeDataString(dataInicio.Value.Date.ToString("yyyy-MM-dd"))}");
        }

        if (dataFim.HasValue)
        {
            queryParameters.Add($"dataFim={Uri.EscapeDataString(dataFim.Value.Date.ToString("yyyy-MM-dd"))}");
        }

        var relativeUrl = "api/pontos/consultar";

        if (queryParameters.Count > 0)
        {
            relativeUrl += "?" + string.Join("&", queryParameters);
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl);

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Não foi possível consultar os registros de ponto. " +
                $"Status: {(int)response.StatusCode}.");
        }

        var registros = await response.Content.ReadFromJsonAsync<List<PontoApiResponse>>(cancellationToken: cancellationToken);

        return registros ?? new List<PontoApiResponse>();
    }

    public async Task<PontoRegistroResultado> RegistrarAsync(
        string token,
        string tipoRegistro,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Tech4HrApi");

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/pontos/registrar");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        request.Content = JsonContent.Create(new { tipoRegistro });

        using var response = await client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                string.IsNullOrWhiteSpace(errorContent)
                    ? "Não foi possível registrar o ponto."
                    : errorContent);
        }

        var registro = await response.Content.ReadFromJsonAsync<PontoRegistroResultado>(cancellationToken: cancellationToken);

        return registro ?? new PontoRegistroResultado();
    }

    public async Task<IReadOnlyList<PontoApiResponse>> ConsultarMeusPontosAsync(
        string token,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("Tech4HrApi");

        var queryParameters = new List<string>();

        if (dataInicio.HasValue)
        {
            queryParameters.Add($"dataInicio={Uri.EscapeDataString(dataInicio.Value.Date.ToString("yyyy-MM-dd"))}");
        }

        if (dataFim.HasValue)
        {
            queryParameters.Add($"dataFim={Uri.EscapeDataString(dataFim.Value.Date.ToString("yyyy-MM-dd"))}");
        }

        var relativeUrl = "api/pontos/meus-pontos";

        if (queryParameters.Count > 0)
        {
            relativeUrl += "?" + string.Join("&", queryParameters);
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl);

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Não foi possível consultar seus registros de ponto. " +
                $"Status: {(int)response.StatusCode}.");
        }

        var registros = await response.Content.ReadFromJsonAsync<List<PontoApiResponse>>(cancellationToken: cancellationToken);

        return registros ?? new List<PontoApiResponse>();
    }
}
