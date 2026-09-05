using ProvaVida.Admin.Application.Queries;
using ProvaVida.Admin.Infrastructure;
using ProvaVida.Admin.Infrastructure.Queries;
using ProvaVida.Admin.Web.Components;
using ProvaVida.Admin.Web.Middleware;
using ProvaVida.Shared.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ── Connection string ──────────────────────────────────────────────────────────
// Prioridade: env var DB_CONNECTION_STRING → appsettings ConnectionStrings:DefaultConnection
var connectionString =
    Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    var startupLogger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("Startup");
    startupLogger.LogWarning(
        "Connection string não configurada. Defina a env var DB_CONNECTION_STRING ou " +
        "appsettings.json > ConnectionStrings:DefaultConnection. " +
        "A aplicação vai inicializar, mas queries de banco vão falhar.");
    connectionString = string.Empty;
}

// ── DI — infraestrutura de dados ──────────────────────────────────────────────
builder.Services.AddSingleton<IDbConnectionFactory>(
    _ => new AdminConnectionFactory(connectionString));

builder.Services.AddScoped<IAdminMetricasQueryService, AdminMetricasQueryService>();

// ── Blazor Server ──────────────────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ── Porta fixa 5019 (SSH tunnel — localhost only) ─────────────────────────────
builder.WebHost.UseUrls("http://localhost:5019");

var app = builder.Build();

// ── Pipeline HTTP ─────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // Sem HSTS — app só é acessada via SSH tunnel (localhost)
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

// Sem UseHttpsRedirection — acesso exclusivo via localhost / SSH tunnel

// Basic Auth protege todas as rotas (menos paths de infraestrutura Blazor)
app.UseMiddleware<BasicAuthMiddleware>();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
