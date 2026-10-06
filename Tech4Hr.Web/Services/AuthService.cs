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

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    return AuthLoginResult.Failure(
                        "Operacionais entram pelo login de funcionário.");
                }

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    return AuthLoginResult.Failure(
                        await LerMensagemDeBloqueioAsync(response, cancellationToken));
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

            // O login administrativo é só de ADMIN. O operacional entra pelo
            // login de funcionário.
            if (!string.Equals(nivelUsuario, "ADMIN", StringComparison.OrdinalIgnoreCase))
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

    public async Task<FuncionarioAuthLoginResult> LoginFuncionarioAsync(
        LoginViewModel model,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (string.IsNullOrWhiteSpace(model.Email))
        {
            return FuncionarioAuthLoginResult.Failure("Informe o e-mail corporativo.");
        }

        if (string.IsNullOrWhiteSpace(model.Senha))
        {
            return FuncionarioAuthLoginResult.Failure("Informe a senha.");
        }

        try
        {
            var client = _httpClientFactory.CreateClient("Tech4HrApi");

            var response = await client.PostAsJsonAsync(
                "api/auth/login-funcionario",
                new FuncionarioAuthLoginRequest
                {
                    EmailCorporativo = model.Email.Trim(),
                    Senha = model.Senha
                },
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return FuncionarioAuthLoginResult.Failure("E-mail ou senha inválidos.");
                }

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    return FuncionarioAuthLoginResult.Failure(
                        await LerMensagemDeBloqueioAsync(response, cancellationToken));
                }

                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return FuncionarioAuthLoginResult.Failure(
                    string.IsNullOrWhiteSpace(errorContent)
                        ? "Falha na autenticação do funcionário."
                        : errorContent);
            }

            var payload = await response.Content.ReadAsStringAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(payload))
            {
                return FuncionarioAuthLoginResult.Failure("Resposta vazia da API durante o login do funcionário.");
            }

            var authResponse = JsonSerializer.Deserialize<FuncionarioAuthApiLoginResponse>(payload, JsonOptions);

            if (authResponse is null || string.IsNullOrWhiteSpace(authResponse.Token) || authResponse.Funcionario is null)
            {
                return FuncionarioAuthLoginResult.Failure("Resposta de autenticação inválida da API.");
            }

            var funcionario = authResponse.Funcionario;

            var user = new FuncionarioAuthenticatedUser
            {
                IdFuncionario = funcionario.IdFuncionario,
                Nome = funcionario.Nome,
                Sobrenome = funcionario.Sobrenome,
                EmailCorporativo = funcionario.EmailCorporativo,
                Ativo = funcionario.Ativo,
                NivelAcesso = string.IsNullOrWhiteSpace(funcionario.NivelAcesso)
                    ? "FUNCIONARIO"
                    : funcionario.NivelAcesso.Trim().ToUpperInvariant()
            };

            return FuncionarioAuthLoginResult.Success(
                authResponse.Token,
                authResponse.Tipo,
                authResponse.ExpiraEm,
                user);
        }
        catch (HttpRequestException)
        {
            return FuncionarioAuthLoginResult.Failure("Não foi possível conectar com a API de autenticação do funcionário.");
        }
        catch (TaskCanceledException)
        {
            return FuncionarioAuthLoginResult.Failure("Tempo limite da autenticação excedido.");
        }
        catch (JsonException)
        {
            return FuncionarioAuthLoginResult.Failure("Resposta da API em formato inválido.");
        }
        catch (Exception)
        {
            return FuncionarioAuthLoginResult.Failure("Ocorreu um erro ao tentar autenticar o funcionário.");
        }
    }

    // A API responde 429 com { "message": "..." } quando a conta foi bloqueada
    // por tentativas demais. A mensagem já diz quanto tempo esperar.
    private static async Task<string> LerMensagemDeBloqueioAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        const string padrao = "Muitas tentativas de login. Tente novamente em alguns minutos.";

        try
        {
            var corpo = await response.Content.ReadAsStringAsync(cancellationToken);

            using var documento = JsonDocument.Parse(corpo);

            if (documento.RootElement.ValueKind == JsonValueKind.Object &&
                documento.RootElement.TryGetProperty("message", out var mensagem) &&
                mensagem.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(mensagem.GetString()))
            {
                return mensagem.GetString()!;
            }
        }
        catch (JsonException)
        {
        }

        return padrao;
    }
}
