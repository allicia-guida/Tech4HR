using Tech4Hr.Web.Filters;
using Tech4Hr.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Se a API recusar o token (conta desativada), volta ao login.
    options.Filters.Add<SessaoExpiradaFilter>();
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<SessaoExpiradaHandler>();

builder.Services.AddHttpClient("Tech4HrApi", client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"]
        ?? throw new InvalidOperationException(
            "A URL da API não foi configurada.");

    client.BaseAddress = new Uri(baseUrl);
})
.AddHttpMessageHandler<SessaoExpiradaHandler>();

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IFuncionarioService, FuncionarioService>();
builder.Services.AddScoped<IPontoService, PontoService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
