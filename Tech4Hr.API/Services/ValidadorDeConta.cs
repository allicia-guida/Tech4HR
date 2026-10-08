using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Tech4Hr.API.Data;

namespace Tech4Hr.API.Services;

/// <summary>
/// Confere no banco, a cada requisição, se a conta do token continua ativa e
/// com o mesmo perfil. Sem isso, desativar ou rebaixar alguém só teria efeito
/// quando o token expirasse.
/// </summary>
public sealed class ValidadorDeConta
{
    private readonly Tech4HrDbContext _context;

    public ValidadorDeConta(Tech4HrDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Devolve null quando a conta é válida, ou o motivo da recusa.
    /// </summary>
    public async Task<string?> VerificarAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var tipoConta = principal.FindFirst("tipo_conta")?.Value;
        var papel = principal.FindFirst("role")?.Value;

        var identificador =
            principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(identificador, out var id))
        {
            return "O token não identifica uma conta válida.";
        }

        switch (tipoConta)
        {
            case "USUARIO":
                var usuario = await _context.Usuarios
                    .AsNoTracking()
                    .Where(u => u.IdUsuario == id)
                    .Select(u => new { u.Ativo, u.NivelUsuario })
                    .FirstOrDefaultAsync(cancellationToken);

                if (usuario is null || !usuario.Ativo)
                {
                    return "Conta desativada.";
                }

                if (!string.Equals(usuario.NivelUsuario, papel, StringComparison.Ordinal))
                {
                    return "O perfil da conta mudou. Entre novamente.";
                }

                return null;

            case "FUNCIONARIO":
                var funcionario = await _context.Funcionarios
                    .AsNoTracking()
                    .Where(f => f.IdFuncionario == id)
                    .Select(f => new { f.Ativo, f.NivelAcesso })
                    .FirstOrDefaultAsync(cancellationToken);

                if (funcionario is null || !funcionario.Ativo)
                {
                    return "Conta desativada.";
                }

                // Rebaixar um operacional vale na hora, sem esperar o token vencer.
                if (!string.Equals(funcionario.NivelAcesso, papel, StringComparison.Ordinal))
                {
                    return "O perfil da conta mudou. Entre novamente.";
                }

                return null;

            default:
                return "Tipo de conta desconhecido.";
        }
    }
}
