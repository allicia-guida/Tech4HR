using System.Net.Http.Headers;
using System.Net.Http.Json;
using Tech4Hr.Web.Models;

namespace Tech4Hr.Web.Services;

public class FuncionarioService : IFuncionarioService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public FuncionarioService(
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IReadOnlyList<FuncionarioApiResponse>> ListarAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var client =
            _httpClientFactory.CreateClient("Tech4HrApi");

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                "api/funcionarios");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        using var response =
            await client.SendAsync(
                request,
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Não foi possível consultar os funcionários. " +
                $"Status: {(int)response.StatusCode}.");
        }

        var funcionarios =
            await response.Content
                .ReadFromJsonAsync<List<FuncionarioApiResponse>>(
                    cancellationToken: cancellationToken);

        return funcionarios
            ?? new List<FuncionarioApiResponse>();
    }

    public async Task<FuncionarioApiResponse?> BuscarPorIdAsync(
        int id,
        string token,
        CancellationToken cancellationToken = default)
    {
        var client =
            _httpClientFactory.CreateClient("Tech4HrApi");

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                $"api/funcionarios/{id}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        using var response =
            await client.SendAsync(
                request,
                cancellationToken);

        if (response.StatusCode ==
            System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Não foi possível consultar o funcionário. " +
                $"Status: {(int)response.StatusCode}.");
        }

        return await response.Content
            .ReadFromJsonAsync<FuncionarioApiResponse>(
                cancellationToken: cancellationToken);
    }

    public async Task<FuncionarioApiResponse> CriarAsync(
        FuncionarioCadastroInputModel model,
        string token,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        var client =
            _httpClientFactory.CreateClient("Tech4HrApi");

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "api/funcionarios");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        request.Content = JsonContent.Create(model);

        using var response =
            await client.SendAsync(
                request,
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Não foi possível cadastrar o funcionário. " +
                $"Status: {(int)response.StatusCode}.");
        }

        return await response.Content
            .ReadFromJsonAsync<FuncionarioApiResponse>(
                cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException(
                "A API respondeu sem os dados do funcionário cadastrado.");
    }

    public async Task<FuncionarioApiResponse?> AtualizarAsync(
        int id,
        FuncionarioEdicaoInputModel model,
        string token,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        var client =
            _httpClientFactory.CreateClient("Tech4HrApi");

        using var request =
            new HttpRequestMessage(
                HttpMethod.Put,
                $"api/funcionarios/{id}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        request.Content = JsonContent.Create(model);

        using var response =
            await client.SendAsync(
                request,
                cancellationToken);

        if (response.StatusCode ==
            System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Não foi possível atualizar o funcionário. " +
                $"Status: {(int)response.StatusCode}.");
        }

        return await response.Content
            .ReadFromJsonAsync<FuncionarioApiResponse>(
                cancellationToken: cancellationToken);
    }

    public async Task<FuncionarioApiResponse?> AlterarStatusAsync(
        int id,
        bool ativo,
        string token,
        CancellationToken cancellationToken = default)
    {
        var client =
            _httpClientFactory.CreateClient("Tech4HrApi");

        using var request =
            new HttpRequestMessage(
                HttpMethod.Patch,
                $"api/funcionarios/{id}/status");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        request.Content = JsonContent.Create(new { ativo });

        using var response =
            await client.SendAsync(
                request,
                cancellationToken);

        if (response.StatusCode ==
            System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Não foi possível alterar o status do funcionário. " +
                $"Status: {(int)response.StatusCode}.");
        }

        return await response.Content
            .ReadFromJsonAsync<FuncionarioApiResponse>(
                cancellationToken: cancellationToken);
    }
}