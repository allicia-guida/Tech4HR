using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Tech4Hr.API.Configurations;
using Tech4Hr.API.Data;
using Tech4Hr.API.DTOs;
using Tech4Hr.API.Models;
using Tech4Hr.API.Services;

namespace Tech4Hr.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly Tech4HrDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly LimiteDeTentativasDeLogin _limite;

    public AuthController(
        Tech4HrDbContext context,
        IConfiguration configuration,
        LimiteDeTentativasDeLogin limite)
    {
        _context = context;
        _configuration = configuration;
        _limite = limite;
    }

    // Resposta de uma conta bloqueada por tentativas demais. O Retry-After vai
    // em segundos e a mensagem em minutos, para a pessoa saber quando voltar.
    private IActionResult TentativasEsgotadas(TimeSpan espera)
    {
        int minutos = Math.Max(1, (int)Math.Ceiling(espera.TotalMinutes));

        if (HttpContext is not null)
        {
            Response.Headers.RetryAfter =
                ((int)Math.Ceiling(espera.TotalSeconds)).ToString();
        }

        return StatusCode(
            StatusCodes.Status429TooManyRequests,
            new
            {
                message = minutos == 1
                    ? "Muitas tentativas de login. Tente novamente em 1 minuto."
                    : $"Muitas tentativas de login. Tente novamente em {minutos} minutos."
            });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginUsuarioDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        string email = dto.Email.Trim().ToLowerInvariant();
        string chaveConta = LimiteDeTentativasDeLogin.Chave("USUARIO", email);

        if (_limite.TempoRestanteDeBloqueio(chaveConta) is { } espera)
        {
            return TentativasEsgotadas(espera);
        }

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == email);

        if (usuario == null || !usuario.Ativo)
        {
            _limite.RegistrarFalha(chaveConta);
            return Unauthorized(new { message = "E-mail ou senha inválidos." });
        }

        var hasher = new PasswordHasher<Usuario>();

        var resultado = hasher.VerifyHashedPassword(
            usuario,
            usuario.SenhaHash,
            dto.Senha
        );

        if (resultado == PasswordVerificationResult.Failed)
        {
            _limite.RegistrarFalha(chaveConta);
            return Unauthorized(new { message = "E-mail ou senha inválidos." });
        }

        _limite.RegistrarSucesso(chaveConta);

        // O login administrativo é só de ADMIN. Operacional agora é um tipo de
        // funcionário e entra pelo login de funcionário.
        if (usuario.NivelUsuario != "ADMIN")
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new { message = "Operacionais entram pelo login de funcionário." });
        }

        var jwt = _configuration
            .GetSection("Jwt")
            .Get<JwtSettings>()
            ?? throw new InvalidOperationException(
                "Configurações JWT não encontradas.");

        var chave = new SymmetricSecurityKey(
            Convert.FromBase64String(jwt.Key)
        );

        var credenciais = new SigningCredentials(
            chave,
            SecurityAlgorithms.HmacSha256
        );

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.IdUsuario.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim("tipo_conta", "USUARIO"),
            new Claim("role", usuario.NivelUsuario),
            new Claim(ClaimTypes.Role, usuario.NivelUsuario)
        };

        var expiracao = DateTime.UtcNow.AddMinutes(jwt.ExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: jwt.Audience,
            claims: claims,
            expires: expiracao,
            signingCredentials: credenciais
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new
        {
            token = tokenString,
            tipo = "Bearer",
            expiraEm = expiracao,
            usuario = new
            {
                usuario.IdUsuario,
                usuario.Nome,
                usuario.Sobrenome,
                usuario.Email,
                usuario.NivelUsuario
            }
        });
    }

    [HttpPost("login-funcionario")]
    public async Task<IActionResult> LoginFuncionario(
        [FromBody] LoginFuncionarioDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        string email = dto.EmailCorporativo.Trim().ToLowerInvariant();
        string chaveConta = LimiteDeTentativasDeLogin.Chave("FUNCIONARIO", email);

        if (_limite.TempoRestanteDeBloqueio(chaveConta) is { } espera)
        {
            return TentativasEsgotadas(espera);
        }

        var funcionario = await _context.Funcionarios
            .FirstOrDefaultAsync(f => f.EmailCorporativo == email);

        if (funcionario == null || !funcionario.Ativo)
        {
            _limite.RegistrarFalha(chaveConta);
            return Unauthorized(new { message = "E-mail ou senha inválidos." });
        }

        var hasher = new PasswordHasher<Funcionario>();

        var resultado = hasher.VerifyHashedPassword(
            funcionario,
            funcionario.SenhaHash,
            dto.Senha
        );

        if (resultado == PasswordVerificationResult.Failed)
        {
            _limite.RegistrarFalha(chaveConta);
            return Unauthorized(new { message = "E-mail ou senha inválidos." });
        }

        _limite.RegistrarSucesso(chaveConta);

        var jwt = _configuration
            .GetSection("Jwt")
            .Get<JwtSettings>()
            ?? throw new InvalidOperationException(
                "Configurações JWT não encontradas.");

        var chave = new SymmetricSecurityKey(
            Convert.FromBase64String(jwt.Key)
        );

        var credenciais = new SigningCredentials(
            chave,
            SecurityAlgorithms.HmacSha256
        );

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, funcionario.IdFuncionario.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, funcionario.EmailCorporativo),
            // O perfil vem do cadastro: FUNCIONARIO ou OPERACIONAL. O operacional
            // continua sendo conta de funcionário (bate ponto) e ganha as rotas
            // de gestão que exigem o papel OPERACIONAL.
            new Claim("tipo_conta", "FUNCIONARIO"),
            new Claim("role", funcionario.NivelAcesso),
            new Claim(ClaimTypes.Role, funcionario.NivelAcesso)
        };

        var expiracao = DateTime.UtcNow.AddMinutes(jwt.ExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: jwt.Audience,
            claims: claims,
            expires: expiracao,
            signingCredentials: credenciais
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new
        {
            token = tokenString,
            tipo = "Bearer",
            expiraEm = expiracao,
            funcionario = new
            {
                funcionario.IdFuncionario,
                funcionario.Nome,
                funcionario.Sobrenome,
                funcionario.EmailCorporativo,
                funcionario.Ativo,
                funcionario.NivelAcesso,
                funcionario.DataAdmissao
            }
        });
    }
}