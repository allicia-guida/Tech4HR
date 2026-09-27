using Tech4Hr.Web.Models;

namespace Tech4Hr.Web.Services;

public interface IAuthService
{
    Task<AuthLoginResult> LoginAsync(LoginViewModel model, CancellationToken cancellationToken = default);
}
