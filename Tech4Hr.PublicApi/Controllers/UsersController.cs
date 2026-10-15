using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tech4Hr.PublicApi.Contracts;
using Tech4Hr.PublicApi.Data;
using Tech4Hr.PublicApi.Models;
using Tech4Hr.PublicApi.Services;

namespace Tech4Hr.PublicApi.Controllers;

[ApiController]
[Authorize(Roles = Roles.Administrador)]
[Route("api/v1/users")]
public sealed class UsersController(PublicApiDbContext db, ICurrentUser currentUser, IAuditService audit) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var role = request.Role.Trim().ToUpperInvariant();
        if (role is not (Roles.Administrador or Roles.Gestor or Roles.Funcionario))
        {
            return BadRequest(new { message = "Perfil inválido." });
        }
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == email, cancellationToken))
        {
            return Conflict(new { message = "E-mail já cadastrado." });
        }
        var user = new User { Name = request.Name.Trim(), Email = email, Role = role };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.Password);
        db.Users.Add(user);
        audit.Add(currentUser.UserId, "CREATE", nameof(User), user.Id);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, new { user.Id, user.Name, user.Email, user.Role, user.Active });
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var users = await db.Users.AsNoTracking().OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name, x.Email, x.Role, x.Active, x.CreatedAtUtc })
            .ToListAsync(cancellationToken);
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await db.Users.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new { x.Id, x.Name, x.Email, x.Role, x.Active, x.CreatedAtUtc })
            .SingleOrDefaultAsync(cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }
}
