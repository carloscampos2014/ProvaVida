using Hangfire;
using ProvaVida.Api.Extensions;
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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Em IntegrationTests, DbConnectionFactory e DatabaseMigrator são registrados pela factory
if (!builder.Environment.IsEnvironment("IntegrationTests"))
{
    builder.Services.AddDatabase(builder.Configuration);
    builder.Services.AddHangfireServices(builder.Configuration);
}

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApplicationServices();

var app = builder.Build();

// Migrations DbUp e jobs Hangfire apenas fora de testes de integração
if (!app.Environment.IsEnvironment("IntegrationTests"))
{
    app.ApplyMigrations();

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

app.UseGlobalExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();

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
    app.MapHangfireDashboard();

app.Run();

public partial class Program { }
