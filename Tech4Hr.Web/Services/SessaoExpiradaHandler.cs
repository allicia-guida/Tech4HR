using System.Net;

namespace Tech4Hr.Web.Services;

/// <summary>
/// Observa as respostas da API. Quando ela recusa um token que o Web enviou
/// (HTTP 401), a conta foi desativada ou a sessão expirou. O handler só marca
/// a requisição atual; quem limpa a sessão e leva ao login é o
/// <see cref="Filters.SessaoExpiradaFilter"/>.
/// </summary>
public sealed class SessaoExpiradaHandler : DelegatingHandler
{
    public const string ChaveDoItem = "Tech4Hr.SessaoExpirada";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public SessaoExpiradaHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var resposta = await base.SendAsync(request, cancellationToken);

        // Um 401 no próprio login (senha errada) não significa sessão expirada:
        // só vale quando a requisição levava um token.
        if (resposta.StatusCode == HttpStatusCode.Unauthorized &&
            request.Headers.Authorization is not null)
        {
            var contexto = _httpContextAccessor.HttpContext;

            if (contexto is not null)
            {
                contexto.Items[ChaveDoItem] = true;
            }
        }

        return resposta;
    }
}
