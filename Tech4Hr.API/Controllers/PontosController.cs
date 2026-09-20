using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Tech4Hr.API.Data;
using Tech4Hr.API.DTOs;
using Tech4Hr.API.Models;

namespace Tech4Hr.API.Controllers;

[ApiController]
[Route("api/pontos")]
public class PontosController : ControllerBase
{
    private readonly Tech4HrDbContext _context;

    public PontosController(Tech4HrDbContext context)
    {
        _context = context;
    }

    [Authorize(Policy = "FuncionarioPolicy")]
    [HttpPost("registrar")]
    public async Task<IActionResult> RegistrarPonto(
        [FromBody] RegistrarPontoDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (!User.Claims.Any(c => c.Type == "tipo_conta" && c.Value == "FUNCIONARIO"))
        {
            return Forbid();
        }

        var idFuncionarioClaim = User.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == JwtRegisteredClaimNames.Sub)
            ?.Value;

        if (!int.TryParse(idFuncionarioClaim, out var idFuncionario))
        {
            return Unauthorized();
        }

        var funcionario = await _context.Funcionarios
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.IdFuncionario == idFuncionario && f.Ativo);

        if (funcionario == null)
        {
            return Unauthorized();
        }

        try
        {
            var parametros = new[]
            {
                new SqlParameter("@IdFuncionario", idFuncionario),
                new SqlParameter("@TipoRegistro", dto.TipoRegistro)
            };

            var resultado = await _context.Database
                .SqlQueryRaw<RegistroPontoResultado>(
                    "EXEC dbo.sp_RegistrarPonto @IdFuncionario, @TipoRegistro",
                    parametros)
                .ToListAsync();

            if (resultado.Count == 0)
            {
                return StatusCode(500, "Não foi possível registrar o ponto.");
            }

            var registro = resultado[0];

            return Ok(new
            {
                registro.IdPonto,
                registro.TipoRegistro,
                registro.DataHora,
                registro.Mensagem
            });
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(ex.InnerException?.Message ?? "Não foi possível registrar o ponto.");
        }
        catch (Exception)
        {
            return StatusCode(500, "Erro ao registrar ponto.");
        }
    }

    [Authorize(Policy = "FuncionarioPolicy")]
    [HttpGet("meus-pontos")]
    public async Task<IActionResult> MeusPontos(
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim)
    {
        var idFuncionarioClaim = User.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == JwtRegisteredClaimNames.Sub)
            ?.Value;

        if (!int.TryParse(idFuncionarioClaim, out var idFuncionario))
        {
            return Unauthorized();
        }

        var query = _context.Pontos
            .AsNoTracking()
            .Where(p => p.IdFuncionario == idFuncionario);

        if (dataInicio.HasValue)
        {
            query = query.Where(p => p.DataPonto >= dataInicio.Value.Date);
        }

        if (dataFim.HasValue)
        {
            query = query.Where(p => p.DataPonto <= dataFim.Value.Date);
        }

        var pontos = await query
            .OrderBy(p => p.DataPonto)
            .Select(p => new
            {
                p.IdPonto,
                p.DataPonto,
                p.Entrada,
                p.SaidaAlmoco,
                p.EntradaAlmoco,
                p.Saida
            })
            .ToListAsync();

        return Ok(pontos);
    }

    [Authorize(Roles = "ADMIN,OPERACIONAL")]
    [HttpGet("consultar")]
    public async Task<IActionResult> ConsultarAdministrativo(
        [FromQuery] int? idFuncionario,
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim)
    {
        var query = _context.Pontos
            .AsNoTracking()
            .Include(p => p.Funcionario)
            .AsQueryable();

        if (idFuncionario.HasValue)
        {
            query = query.Where(p => p.IdFuncionario == idFuncionario.Value);
        }

        if (dataInicio.HasValue)
        {
            query = query.Where(p => p.DataPonto >= dataInicio.Value.Date);
        }

        if (dataFim.HasValue)
        {
            query = query.Where(p => p.DataPonto <= dataFim.Value.Date);
        }

        var pontos = await query
            .OrderBy(p => p.DataPonto)
            .Select(p => new
            {
                p.IdPonto,
                p.IdFuncionario,
                p.DataPonto,
                p.Entrada,
                p.SaidaAlmoco,
                p.EntradaAlmoco,
                p.Saida,
                Funcionario = new
                {
                    p.Funcionario.IdFuncionario,
                    p.Funcionario.Nome,
                    p.Funcionario.Sobrenome
                }
            })
            .ToListAsync();

        return Ok(pontos);
    }
}

public sealed class RegistroPontoResultado
{
    public int IdPonto { get; set; }
    public string TipoRegistro { get; set; } = string.Empty;
    public DateTimeOffset DataHora { get; set; }
    public string Mensagem { get; set; } = string.Empty;
}
