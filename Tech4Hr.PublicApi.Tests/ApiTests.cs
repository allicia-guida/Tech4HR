using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Tech4Hr.PublicApi.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"tech4hr-api-{Guid.NewGuid():N}.db");

    public ApiFactory()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__PublicApi", $"Data Source={_databasePath}");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "Tech4Hr.Tests");
        Environment.SetEnvironmentVariable("Jwt__Audience", "Tech4Hr.Tests.Client");
        Environment.SetEnvironmentVariable("Jwt__Key", "AAECAwQFBgcICQoLDA0ODxAREhMUFRYXGBkaGxwdHh8=");
        Environment.SetEnvironmentVariable("Jwt__ExpirationMinutes", "15");
        Environment.SetEnvironmentVariable("BootstrapAdmin__Name", "Administrador Teste");
        Environment.SetEnvironmentVariable("BootstrapAdmin__Email", "admin@tech4hr.test");
        Environment.SetEnvironmentVariable("BootstrapAdmin__Password", "SenhaTesteSegura!2026");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:PublicApi"] = $"Data Source={_databasePath}",
                ["Jwt:Issuer"] = "Tech4Hr.Tests",
                ["Jwt:Audience"] = "Tech4Hr.Tests.Client",
                ["Jwt:Key"] = "AAECAwQFBgcICQoLDA0ODxAREhMUFRYXGBkaGxwdHh8=",
                ["Jwt:ExpirationMinutes"] = "15",
                ["BootstrapAdmin:Name"] = "Administrador Teste",
                ["BootstrapAdmin:Email"] = "admin@tech4hr.test",
                ["BootstrapAdmin:Password"] = "SenhaTesteSegura!2026"
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__PublicApi", null);
            Environment.SetEnvironmentVariable("Jwt__Issuer", null);
            Environment.SetEnvironmentVariable("Jwt__Audience", null);
            Environment.SetEnvironmentVariable("Jwt__Key", null);
            Environment.SetEnvironmentVariable("Jwt__ExpirationMinutes", null);
            Environment.SetEnvironmentVariable("BootstrapAdmin__Name", null);
            Environment.SetEnvironmentVariable("BootstrapAdmin__Email", null);
            Environment.SetEnvironmentVariable("BootstrapAdmin__Password", null);
            SqliteConnection.ClearAllPools();
            if (File.Exists(_databasePath)) File.Delete(_databasePath);
            if (File.Exists(_databasePath + "-shm")) File.Delete(_databasePath + "-shm");
            if (File.Exists(_databasePath + "-wal")) File.Delete(_databasePath + "-wal");
        }
    }
}

public sealed class ApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost")
    });

    [Fact]
    public async Task Health_returns_ok()
    {
        var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Protected_endpoint_rejects_anonymous_request()
    {
        var response = await _client.GetAsync("/api/v1/points/history");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_and_idempotent_point_registration_work()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "admin@tech4hr.test",
            password = "SenhaTesteSegura!2026"
        });
        loginResponse.EnsureSuccessStatusCode();
        using var loginJson = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        var token = loginJson.RootElement.GetProperty("token").GetString();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var request = new
        {
            type = "ENTRADA",
            latitude = -23.5505,
            longitude = -46.6333,
            accuracyMeters = 10,
            idempotencyKey = "operation-001"
        };
        var first = await _client.PostAsJsonAsync("/api/v1/points", request);
        var replay = await _client.PostAsJsonAsync("/api/v1/points", request);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, replay.StatusCode);
    }
}
