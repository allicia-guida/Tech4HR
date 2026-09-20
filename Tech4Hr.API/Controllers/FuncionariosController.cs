using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Tech4Hr.API.Data;
using Tech4Hr.API.DTOs;
using Tech4Hr.API.Models;

namespace Tech4Hr.API.Controllers;

[ApiController]
[Route("api/funcionarios")]
public class FuncionariosController : ControllerBase
{
    private readonly Tech4HrDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public FuncionariosController(
        Tech4HrDbContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpPost]
    public async Task<IActionResult> Cadastrar(
        [FromBody] CadastrarFuncionarioDto dto)
    {
        // Temporário: cadastro liberado apenas no ambiente local
        // até implementarmos autenticação e autorização.
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        string nome = dto.Nome.Trim();
        string sobrenome = dto.Sobrenome.Trim();
        string email = dto.EmailCorporativo.Trim().ToLowerInvariant();
        string cpf = dto.CPF.Trim();

        if (string.IsNullOrWhiteSpace(nome) ||
            string.IsNullOrWhiteSpace(sobrenome))
        {
            return BadRequest("Nome e sobrenome são obrigatórios.");
        }

        bool emailExiste = await _context.Funcionarios
            .AnyAsync(f => f.EmailCorporativo == email);

        if (emailExiste)
        {
            return Conflict("Este e-mail já está cadastrado.");
        }

        bool cpfExiste = await _context.Funcionarios
            .AnyAsync(f => f.CPF == cpf);

        if (cpfExiste)
        {
            return Conflict("Este CPF já está cadastrado.");
        }

        var funcionario = new Funcionario
        {
            Nome = nome,
            Sobrenome = sobrenome,
            EmailCorporativo = email,
            CPF = cpf,
            DataAdmissao = dto.DataAdmissao!.Value,
            Ativo = true
        };

        var passwordHasher = new PasswordHasher<Funcionario>();

        funcionario.SenhaHash = passwordHasher.HashPassword(
            funcionario,
            dto.Senha
        );

        _context.Funcionarios.Add(funcionario);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sqlEx &&
                  (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            return Conflict("E-mail ou CPF já cadastrado.");
        }

        return StatusCode(StatusCodes.Status201Created, new
        {
            funcionario.IdFuncionario,
            funcionario.Nome,
            funcionario.Sobrenome,
            funcionario.EmailCorporativo,
            funcionario.CPF,
            funcionario.DataAdmissao,
            funcionario.Ativo
        });
    }

    [HttpGet]
public async Task<IActionResult> Listar()
{
    // Temporário: restringe a consulta ao ambiente de desenvolvimento.
    if (!_environment.IsDevelopment())
    {
        return NotFound();
    }

    var funcionarios = await _context.Funcionarios
        .AsNoTracking()
        .OrderBy(f => f.Nome)
        .ThenBy(f => f.Sobrenome)
        .Select(f => new
        {
            f.IdFuncionario,
            f.Nome,
            f.Sobrenome,
            f.EmailCorporativo,
            f.CPF,
            f.DataAdmissao,
            f.Ativo
        })
        .ToListAsync();

    return Ok(funcionarios);
}

}