using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Tech4Hr.API.Configurations;
using Tech4Hr.API.Data;
using Tech4Hr.API.Setup;

var builder = WebApplication.CreateBuilder(args);

// Conexão com o Azure SQL
var connectionString = builder.Configuration
    .GetConnectionString("Tech4HrDB")
    ?? throw new InvalidOperationException(
        "Connection string Tech4HrDB nao configurada.");

builder.Services.AddDbContext<Tech4HrDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configurações JWT
var jwtSettings = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "Configuracoes JWT nao encontradas.");

if (string.IsNullOrWhiteSpace(jwtSettings.Issuer) ||
    string.IsNullOrWhiteSpace(jwtSettings.Audience) ||
    string.IsNullOrWhiteSpace(jwtSettings.Key))
{
    throw new InvalidOperationException(
        "Configuracoes JWT incompletas.");
}

byte[] jwtKey;

try
{
    jwtKey = Convert.FromBase64String(jwtSettings.Key);
}
catch (FormatException)
{
    throw new InvalidOperationException(
        "A chave JWT deve estar em Base64.");
}

if (jwtKey.Length < 32)
{
    throw new InvalidOperationException(
        "A chave JWT deve conter pelo menos 32 bytes.");
}

if (jwtSettings.ExpirationMinutes <= 0)
{
    throw new InvalidOperationException(
        "O tempo de expiracao do JWT deve ser maior que zero.");
}

// Autenticação
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,

                ValidateLifetime = true,
                RequireExpirationTime = true,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(jwtKey),

                RequireSignedTokens = true,

                RoleClaimType = "role",

                ClockSkew = TimeSpan.Zero
            };
    });

// Autorização
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UsuarioPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("tipo_conta", "USUARIO");
    });

    options.AddPolicy("FuncionarioPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("tipo_conta", "FUNCIONARIO");
    });

    options.AddPolicy("AdminPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("tipo_conta", "USUARIO");
        policy.RequireRole("ADMIN");
    });
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Inicialização manual do primeiro administrador.
// Execute somente em um ambiente local e controlado.
if (args.Contains("--criar-admin-inicial"))
{
    if (!app.Environment.IsDevelopment())
    {
        throw new InvalidOperationException(
            "A inicializacao do ADMIN so pode ser executada em Development.");
    }

    using var scope = app.Services.CreateScope();

    var context = scope.ServiceProvider
        .GetRequiredService<Tech4HrDbContext>();

    // Não iniciar o processo se já existir um ADMIN.
    if (await context.Usuarios.AnyAsync(u => u.NivelUsuario == "ADMIN"))
    {
        Console.WriteLine("Ja existe um ADMIN cadastrado.");
        return;
    }

    Console.Write("Nome: ");
    string nome = Console.ReadLine() ?? "";

    Console.Write("Sobrenome: ");
    string sobrenome = Console.ReadLine() ?? "";

    Console.Write("E-mail: ");
    string email = Console.ReadLine() ?? "";

    Console.Write("Senha (nao sera exibida): ");

    var senha = new StringBuilder();

    while (true)
    {
        var tecla = Console.ReadKey(intercept: true);

        if (tecla.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            break;
        }

        if (tecla.Key == ConsoleKey.Backspace)
        {
            if (senha.Length > 0)
            {
                senha.Length--;
            }

            continue;
        }

        if (tecla.KeyChar != '\0')
        {
            senha.Append(tecla.KeyChar);
        }
    }

    try
    {
        await CriarAdminInicial.ExecutarAsync(
            context,
            nome,
            sobrenome,
            email,
            senha.ToString()
        );

        Console.WriteLine("Administrador inicial criado com sucesso!");
    }
    catch (Exception ex) when (
        ex is ArgumentException ||
        ex is InvalidOperationException)
    {
        Console.WriteLine($"Nao foi possivel criar o ADMIN: {ex.Message}");
    }
    finally
    {
        senha.Clear();
    }

    return;
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();