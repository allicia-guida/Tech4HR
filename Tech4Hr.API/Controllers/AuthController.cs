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

namespace Tech4Hr.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly Tech4HrDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(
        Tech4HrDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
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

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == email);

        if (usuario == null || !usuario.Ativo)
        {
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
            return Unauthorized(new { message = "E-mail ou senha inválidos." });
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

        var funcionario = await _context.Funcionarios
            .FirstOrDefaultAsync(f => f.EmailCorporativo == email);

        if (funcionario == null || !funcionario.Ativo)
        {
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
            return Unauthorized(new { message = "E-mail ou senha inválidos." });
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
            new Claim(JwtRegisteredClaimNames.Sub, funcionario.IdFuncionario.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, funcionario.EmailCorporativo),
            new Claim("tipo_conta", "FUNCIONARIO"),
            new Claim("role", "FUNCIONARIO"),
            new Claim(ClaimTypes.Role, "FUNCIONARIO")
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
                funcionario.Ativo
            }
        });
    }
}