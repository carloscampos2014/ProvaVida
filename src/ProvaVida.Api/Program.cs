using Dapper;
using Hangfire;
using Microsoft.AspNetCore.Authentication;
using Scalar.AspNetCore;
using ProvaVida.Api.Extensions;
using ProvaVida.Api.Filters;
using ProvaVida.Api.Middleware;
using ProvaVida.Infrastructure.Jobs;
using ProvaVida.Infrastructure.Persistence;
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
builder.Services.AddApiRateLimiting();

// Basic Auth para o painel Admin — adiciona o scheme sem sobrescrever o esquema padrão (Bearer)
builder.Services.AddAuthentication()
    .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("BasicAuth", null);
builder.Services.AddAuthorization();

var app = builder.Build();

// Migrations DbUp apenas fora de testes de integração
if (!app.Environment.IsEnvironment("IntegrationTests"))
{
    app.ApplyMigrations();
}

app.UseGlobalExceptionHandler();
app.UseMiddleware<BruteForceMiddleware>();

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
app.UseRateLimiter();

app.MapControllers();

// Health check — verifica conectividade real com o banco PostgreSQL
// Retorna 503 se o banco estiver fora — garante que CI/CD detecta deploys quebrados
app.MapGet("/health", async (DbConnectionFactory db) =>
{
    try
    {
        using var conn = db.CreateConnection();
        await conn.ExecuteScalarAsync<int>("SELECT 1");
        return Results.Ok(new { status = "healthy", timestamp = DateTimeOffset.UtcNow });
    }
    catch (Exception ex)
    {
        return Results.Json(
            new { status = "unhealthy", error = ex.Message, timestamp = DateTimeOffset.UtcNow },
            statusCode: 503);
    }
})
.AllowAnonymous();

if (!app.Environment.IsEnvironment("IntegrationTests"))
{
    app.MapHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = [new HangfireBasicAuthFilter(app.Configuration)]
    });

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

    RecurringJob.AddOrUpdate<BackupDatabaseJob>(
        "backup-diario",
        job => job.ExecutarAsync(),
        "55 23 * * *",
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
}

app.Run();

public partial class Program { }
