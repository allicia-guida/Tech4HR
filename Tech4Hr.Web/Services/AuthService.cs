using System.Net;
using System.Text.Json;
using Tech4Hr.Web.Models;

namespace Tech4Hr.Web.Services;

public class AuthService : IAuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AuthService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<AuthLoginResult> LoginAsync(
        LoginViewModel model,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (string.IsNullOrWhiteSpace(model.Email))
        {
            return AuthLoginResult.Failure("Informe o e-mail.");
        }

        if (string.IsNullOrWhiteSpace(model.Senha))
        {
            return AuthLoginResult.Failure("Informe a senha.");
        }

        try
        {
            var client = _httpClientFactory.CreateClient("Tech4HrApi");

            var response = await client.PostAsJsonAsync(
                "api/auth/login",
                new AuthLoginRequest
                {
                    Email = model.Email.Trim(),
                    Senha = model.Senha
                },
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return AuthLoginResult.Failure("E-mail ou senha inválidos.");
                }

                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return AuthLoginResult.Failure(
                    string.IsNullOrWhiteSpace(errorContent)
                        ? "Falha na autenticação da API."
                        : errorContent);
            }

            var payload = await response.Content.ReadAsStringAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(payload))
            {
                return AuthLoginResult.Failure("Resposta vazia da API durante o login.");
            }

            var authResponse = JsonSerializer.Deserialize<AuthApiLoginResponse>(payload, JsonOptions);

            if (authResponse is null || string.IsNullOrWhiteSpace(authResponse.Token) || authResponse.Usuario is null)
            {
                return AuthLoginResult.Failure("Resposta de autenticação inválida da API.");
            }

            var nivelUsuario = authResponse.Usuario.NivelUsuario;

            if (!string.Equals(nivelUsuario, "ADMIN", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(nivelUsuario, "OPERACIONAL", StringComparison.OrdinalIgnoreCase))
            {
                return AuthLoginResult.Failure("Usuário não possui perfil administrativo.");
            }

            var user = new AuthenticatedUser
            {
                IdUsuario = authResponse.Usuario.IdUsuario,
                Nome = authResponse.Usuario.Nome,
                Sobrenome = authResponse.Usuario.Sobrenome,
                Email = authResponse.Usuario.Email,
                NivelUsuario = authResponse.Usuario.NivelUsuario
            };

            return AuthLoginResult.Success(
                authResponse.Token,
                authResponse.Tipo,
                authResponse.ExpiraEm,
                user);
        }
        catch (HttpRequestException)
        {
            return AuthLoginResult.Failure("Não foi possível conectar com a API de autenticação.");
        }
        catch (TaskCanceledException)
        {
            return AuthLoginResult.Failure("Tempo limite da autenticação excedido.");
        }
        catch (JsonException)
        {
            return AuthLoginResult.Failure("Resposta da API em formato inválido.");
        }
        catch (Exception)
        {
            return AuthLoginResult.Failure("Ocorreu um erro ao tentar autenticar o usuário.");
        }
    }
}
