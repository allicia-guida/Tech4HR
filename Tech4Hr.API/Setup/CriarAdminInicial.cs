using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tech4Hr.API.Data;
using Tech4Hr.API.Models;

namespace Tech4Hr.API.Setup;

public static class CriarAdminInicial
{
    public static async Task ExecutarAsync(
        Tech4HrDbContext context,
        string nome,
        string sobrenome,
        string email,
        string senha)
    {
        bool existeAdmin = await context.Usuarios
            .AnyAsync(u => u.NivelUsuario == "ADMIN");

        if (existeAdmin)
        {
            throw new InvalidOperationException(
                "Ja existe um administrador cadastrado.");
        }

        email = email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(nome) ||
            string.IsNullOrWhiteSpace(sobrenome) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(senha))
        {
            throw new ArgumentException(
                "Todos os campos sao obrigatorios.");
        }

        if (senha.Length < 12)
        {
            throw new ArgumentException(
                "A senha deve conter pelo menos 12 caracteres.");
        }

        bool emailExiste = await context.Usuarios
            .AnyAsync(u => u.Email == email);

        if (emailExiste)
        {
            throw new InvalidOperationException(
                "Este e-mail ja esta cadastrado.");
        }

        var usuario = new Usuario
        {
            Nome = nome.Trim(),
            Sobrenome = sobrenome.Trim(),
            Email = email,
            NivelUsuario = "ADMIN",
            Ativo = true
        };

        var hasher = new PasswordHasher<Usuario>();

        usuario.SenhaHash = hasher.HashPassword(
            usuario,
            senha
        );

        context.Usuarios.Add(usuario);

        await context.SaveChangesAsync();
    }
}