using System.Security.Claims;

namespace Tech4Hr.PublicApi.Services;

public interface ICurrentUser
{
    Guid UserId { get; }
}

public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid UserId
    {
        get
        {
            var value = accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? accessor.HttpContext?.User.FindFirstValue("sub");
            return Guid.TryParse(value, out var id)
                ? id
                : throw new UnauthorizedAccessException("Usuário não autenticado.");
        }
    }
}
