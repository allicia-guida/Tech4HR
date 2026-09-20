using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Tech4Hr.API.Data;
using Tech4Hr.API.DTOs;
using Tech4Hr.API.Models;

namespace Tech4Hr.API.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly Tech4HrDbContext _context;

    public UsuariosController(Tech4HrDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost]
    public async Task<IActionResult> Cadastrar(
        [FromBody] CadastrarUsuarioDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        string nome = dto.Nome.Trim();
        string sobrenome = dto.Sobrenome.Trim();
        string email = dto.Email.Trim().ToLowerInvariant();
        string nivel = dto.NivelUsuario.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(nome) ||
            string.IsNullOrWhiteSpace(sobrenome))
        {
            return BadRequest("Nome e sobrenome são obrigatórios.");
        }

        if (nivel != "ADMIN" && nivel != "OPERACIONAL")
        {
            return BadRequest("Nivel do usuário inválido.");
        }

        bool emailExiste = await _context.Usuarios
            .AnyAsync(u => u.Email == email);

        if (emailExiste)
        {
            return Conflict("Este e-mail já está cadastrado.");
        }

        var usuario = new Usuario
        {
            Nome = nome,
            Sobrenome = sobrenome,
            Email = email,
            Ativo = true,
            NivelUsuario = nivel
        };

        var hasher = new PasswordHasher<Usuario>();
        usuario.SenhaHash = hasher.HashPassword(usuario, dto.Senha);

        _context.Usuarios.Add(usuario);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sqlEx &&
                  (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            return Conflict("E-mail já cadastrado.");
        }

        return StatusCode(StatusCodes.Status201Created, new
        {
            usuario.IdUsuario,
            usuario.Nome,
            usuario.Sobrenome,
            usuario.Email,
            usuario.NivelUsuario,
            usuario.Ativo
        });
    }

    [Authorize(Roles = "ADMIN")]
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var usuarios = await _context.Usuarios
            .AsNoTracking()
            .OrderBy(u => u.Nome)
            .ThenBy(u => u.Sobrenome)
            .Select(u => new
            {
                u.IdUsuario,
                u.Nome,
                u.Sobrenome,
                u.Email,
                u.NivelUsuario,
                u.Ativo
            })
            .ToListAsync();

        return Ok(usuarios);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> BuscarPorId(int id)
    {
        if (id <= 0)
        {
            return BadRequest("O ID deve ser maior que zero.");
        }

        var usuario = await _context.Usuarios
            .AsNoTracking()
            .Where(u => u.IdUsuario == id)
            .Select(u => new
            {
                u.IdUsuario,
                u.Nome,
                u.Sobrenome,
                u.Email,
                u.NivelUsuario,
                u.Ativo
            })
            .FirstOrDefaultAsync();

        if (usuario == null)
        {
            return NotFound("Usuário não encontrado.");
        }

        return Ok(usuario);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Editar(
        int id,
        [FromBody] EditarUsuarioDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (id <= 0)
        {
            return BadRequest("O ID deve ser maior que zero.");
        }

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.IdUsuario == id);

        if (usuario == null)
        {
            return NotFound("Usuário não encontrado.");
        }

        string nome = dto.Nome.Trim();
        string sobrenome = dto.Sobrenome.Trim();
        string email = dto.Email.Trim().ToLowerInvariant();
        string? nivel = dto.NivelUsuario is null
            ? null
            : dto.NivelUsuario.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(nome) ||
            string.IsNullOrWhiteSpace(sobrenome))
        {
            return BadRequest("Nome e sobrenome são obrigatórios.");
        }

        if (!string.IsNullOrWhiteSpace(nivel) &&
            nivel != "ADMIN" &&
            nivel != "OPERACIONAL")
        {
            return BadRequest("Nivel do usuário inválido.");
        }

        bool emailExiste = await _context.Usuarios
            .AnyAsync(u =>
                u.Email == email &&
                u.IdUsuario != id);

        if (emailExiste)
        {
            return Conflict("Este e-mail já está cadastrado.");
        }

        if (usuario.NivelUsuario == "ADMIN" &&
            nivel == "OPERACIONAL" &&
            !await ExisteOutroAdminAtivo(id))
        {
            return Conflict("O último ADMIN ativo não pode ser rebaixado.");
        }

        usuario.Nome = nome;
        usuario.Sobrenome = sobrenome;
        usuario.Email = email;

        if (!string.IsNullOrWhiteSpace(nivel))
        {
            usuario.NivelUsuario = nivel;
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sqlEx &&
                  (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            return Conflict("E-mail já cadastrado.");
        }

        return Ok(new
        {
            usuario.IdUsuario,
            usuario.Nome,
            usuario.Sobrenome,
            usuario.Email,
            usuario.NivelUsuario,
            usuario.Ativo
        });
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> AlterarStatus(
        int id,
        [FromBody] AlterarUsuarioStatusDto dto)
    {
        if (id <= 0)
        {
            return BadRequest("O ID deve ser maior que zero.");
        }

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.IdUsuario == id);

        if (usuario == null)
        {
            return NotFound("Usuário não encontrado.");
        }

        if (usuario.NivelUsuario == "ADMIN" &&
            !dto.Ativo &&
            !await ExisteOutroAdminAtivo(id))
        {
            return Conflict("O último ADMIN ativo não pode ser desativado.");
        }

        usuario.Ativo = dto.Ativo;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            usuario.IdUsuario,
            usuario.Nome,
            usuario.Sobrenome,
            usuario.NivelUsuario,
            usuario.Ativo
        });
    }

    private async Task<bool> ExisteOutroAdminAtivo(int idUsuario)
    {
        return await _context.Usuarios
            .AnyAsync(u =>
                u.IdUsuario != idUsuario &&
                u.NivelUsuario == "ADMIN" &&
                u.Ativo);
    }
}
