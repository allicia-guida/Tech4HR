using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Tech4Hr.PublicApi.Configuration;

namespace Tech4Hr.PublicApi.Services;

public interface IEmailSender
{
    Task SendPasswordResetAsync(string recipient, string resetUrl, CancellationToken cancellationToken);
}

public sealed class SmtpEmailSender(IOptions<SmtpSettings> options) : IEmailSender
{
    private readonly SmtpSettings _settings = options.Value;

    public async Task SendPasswordResetAsync(string recipient, string resetUrl, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.Host) || string.IsNullOrWhiteSpace(_settings.From))
        {
            throw new InvalidOperationException("O envio de recuperação de senha não está configurado.");
        }

        using var message = new MailMessage(_settings.From, recipient)
        {
            Subject = "Recuperação de senha Tech4HR",
            Body = $"Use este link para redefinir sua senha: {resetUrl}",
            IsBodyHtml = false
        };
        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl
        };
        if (!string.IsNullOrWhiteSpace(_settings.Username))
        {
            client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
        }
        cancellationToken.ThrowIfCancellationRequested();
        await client.SendMailAsync(message, cancellationToken);
    }
}
