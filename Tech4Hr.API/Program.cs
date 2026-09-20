using Microsoft.EntityFrameworkCore;
using Tech4Hr.API.Data;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// CONEXAO COM O AZURE SQL
// ==========================================

var connectionString = builder.Configuration
    .GetConnectionString("Tech4HrDB")
    ?? throw new InvalidOperationException(
        "Connection string Tech4HrDB nao configurada."
    );

builder.Services.AddDbContext<Tech4HrDbContext>(options =>
    options.UseSqlServer(connectionString)
);

// ==========================================
// SERVICOS DA API
// ==========================================

builder.Services.AddControllers();

builder.Services.AddOpenApi();

// ==========================================
// CONSTRUCAO DA APLICACAO
// ==========================================

var app = builder.Build();

// ==========================================
// CONFIGURACOES DE DESENVOLVIMENTO
// ==========================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// ==========================================
// MIDDLEWARES
// ==========================================

app.UseHttpsRedirection();

app.UseAuthorization();

// ==========================================
// ROTAS DOS CONTROLLERS
// ==========================================

app.MapControllers();

app.Run();