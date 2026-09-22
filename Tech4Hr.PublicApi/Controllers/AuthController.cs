using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Tech4Hr.PublicApi.Configuration;
using Tech4Hr.PublicApi.Contracts;
using Tech4Hr.PublicApi.Data;
using Tech4Hr.PublicApi.Models;
using Tech4Hr.PublicApi.Services;

namespace Tech4Hr.PublicApi.Controllers;

[ApiController]
[EnableRateLimiting("auth")]
[Route("api/v1/auth")]
public sealed class AuthController(
    PublicApiDbContext db,
    ITokenService tokenService,
    IEmailSender emailSender,
    IOptions<PasswordResetSettings> resetOptions,
    IAuditService audit,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == email, cancellationToken);
        if (user is null || !user.Active)
        {
            return Unauthorized(new { message = "E-mail ou senha inválidos." });
        }
        var hasher = new PasswordHasher<User>();
        var verification = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { message = "E-mail ou senha inválidos." });
        }
        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = hasher.HashPassword(user, request.Password);
            await db.SaveChangesAsync(cancellationToken);
        }
        var token = tokenService.Create(user);
        audit.Add(user.Id, "LOGIN", nameof(User), user.Id);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new
        {
            token = token.Token,
            tokenType = "Bearer",
            expiresAtUtc = token.ExpiresAtUtc,
            user = new { user.Id, user.Name, user.Email, user.Role }
        });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == email && x.Active, cancellationToken);
        if (user is not null)
        {
            var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            var resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                TokenHash = HashToken(rawToken),
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(resetOptions.Value.ExpirationMinutes)
            };
            db.PasswordResetTokens.Add(resetToken);
            await db.SaveChangesAsync(cancellationToken);
            var url = $"{resetOptions.Value.AppUrl}?token={Uri.EscapeDataString(rawToken)}";
            try
            {
                await emailSender.SendPasswordResetAsync(user.Email, url, cancellationToken);
            }
            catch (Exception exception) when (exception is InvalidOperationException or SmtpException)
            {
                db.PasswordResetTokens.Remove(resetToken);
                await db.SaveChangesAsync(cancellationToken);
                logger.LogWarning("Não foi possível enviar uma recuperação de senha.");
            }
        }
        return Accepted(new { message = "Se o e-mail estiver cadastrado, as instruções serão enviadas." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var hash = HashToken(request.Token);
        var reset = await db.PasswordResetTokens.Include(x => x.User)
            .SingleOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);
        if (reset is null || reset.UsedAtUtc is not null || reset.ExpiresAtUtc <= DateTime.UtcNow || !reset.User.Active)
        {
            return BadRequest(new { message = "Token inválido ou expirado." });
        }
        var hasher = new PasswordHasher<User>();
        reset.User.PasswordHash = hasher.HashPassword(reset.User, request.NewPassword);
        reset.UsedAtUtc = DateTime.UtcNow;
        audit.Add(reset.UserId, "PASSWORD_RESET", nameof(User), reset.UserId);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
