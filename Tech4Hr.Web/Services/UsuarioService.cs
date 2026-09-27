using System.Net.Http.Headers;
using System.Net.Http.Json;
using Tech4Hr.Web.Models;

namespace Tech4Hr.Web.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public UsuarioService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IReadOnlyList<UsuarioApiResponse>> ListarAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var client =
            _httpClientFactory.CreateClient("Tech4HrApi");

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                "api/usuarios");

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
                $"Não foi possível consultar os usuários. " +
                $"Status: {(int)response.StatusCode}.");
        }

        var usuarios =
            await response.Content
                .ReadFromJsonAsync<List<UsuarioApiResponse>>(
                    cancellationToken: cancellationToken);

        return usuarios ?? new List<UsuarioApiResponse>();
    }

    public async Task<UsuarioApiResponse?> BuscarPorIdAsync(
        int id,
        string token,
        CancellationToken cancellationToken = default)
    {
        var client =
            _httpClientFactory.CreateClient("Tech4HrApi");

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                $"api/usuarios/{id}");

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
                $"Não foi possível consultar o usuário. " +
                $"Status: {(int)response.StatusCode}.");
        }

        return await response.Content
            .ReadFromJsonAsync<UsuarioApiResponse>(
                cancellationToken: cancellationToken);
    }

    public async Task<UsuarioApiResponse> CriarAsync(
        UsuarioCadastroInputModel model,
        string token,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        var client =
            _httpClientFactory.CreateClient("Tech4HrApi");

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "api/usuarios");

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
                $"Não foi possível cadastrar o usuário. " +
                $"Status: {(int)response.StatusCode}.");
        }

        return await response.Content
            .ReadFromJsonAsync<UsuarioApiResponse>(
                cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException(
                "A API respondeu sem os dados do usuário cadastrado.");
    }

    public async Task<UsuarioApiResponse?> AtualizarAsync(
        int id,
        UsuarioEdicaoInputModel model,
        string token,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        var client =
            _httpClientFactory.CreateClient("Tech4HrApi");

        using var request =
            new HttpRequestMessage(
                HttpMethod.Put,
                $"api/usuarios/{id}");

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
                $"Não foi possível atualizar o usuário. " +
                $"Status: {(int)response.StatusCode}.");
        }

        return await response.Content
            .ReadFromJsonAsync<UsuarioApiResponse>(
                cancellationToken: cancellationToken);
    }

    public async Task<UsuarioApiResponse?> AlterarStatusAsync(
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
                $"api/usuarios/{id}/status");

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
                $"Não foi possível alterar o status do usuário. " +
                $"Status: {(int)response.StatusCode}.");
        }

        return await response.Content
            .ReadFromJsonAsync<UsuarioApiResponse>(
                cancellationToken: cancellationToken);
    }
}
