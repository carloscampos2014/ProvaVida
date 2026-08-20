using Hangfire;
using Microsoft.AspNetCore.Authentication;
using Scalar.AspNetCore;
using ProvaVida.Api.Extensions;
using ProvaVida.Api.Filters;
using ProvaVida.Infrastructure.Jobs;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Notifica o systemd quando o app está pronto (Type=notify no service)
builder.Host.UseSystemd();

// Serilog
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration));

// Serviços
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Em IntegrationTests, DbConnectionFactory e DatabaseMigrator são registrados pela factory
if (!builder.Environment.IsEnvironment("IntegrationTests"))
{
    builder.Services.AddDatabase(builder.Configuration);
    builder.Services.AddHangfireServices(builder.Configuration);
}

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApplicationServices();

// Basic Auth para o painel Admin
builder.Services.AddAuthentication("BasicAuth")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("BasicAuth", null);
builder.Services.AddAuthorization();

var app = builder.Build();

// Migrations DbUp apenas fora de testes de integração
if (!app.Environment.IsEnvironment("IntegrationTests"))
{
    app.ApplyMigrations();
}

app.UseGlobalExceptionHandler();

// Scalar — disponível em /scalar
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "ProvaVida API";
    options.Theme = ScalarTheme.Purple;
});

if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = []
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check — usado pelo GitHub Actions para verificar o deploy
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
   .AllowAnonymous();

if (!app.Environment.IsEnvironment("IntegrationTests"))
{
    app.MapHangfireDashboard();

    // Registrar jobs após o Hangfire middleware estar configurado (JobStorage já inicializado)
    RecurringJob.AddOrUpdate<VerificacaoInatividadeJob>(
        "verificacao-inatividade",
        job => job.ExecutarAsync(),
        "50 23 * * *",
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

    RecurringJob.AddOrUpdate<DispararAlertaJob>(
        "disparar-alerta",
        job => job.ExecutarAsync(),
        "0 * * * *",
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
}

app.Run();

public partial class Program { }
