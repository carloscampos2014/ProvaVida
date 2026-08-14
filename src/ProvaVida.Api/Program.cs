using Hangfire;
using ProvaVida.Api.Extensions;
using ProvaVida.Infrastructure.Jobs;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration));

// Serviços
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddHangfireServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApplicationServices();

var app = builder.Build();

// Migrations DbUp
app.ApplyMigrations();

app.UseGlobalExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI();

// Hangfire Dashboard (apenas em Development — em produção, proteger com autenticação)
if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = [] // sem autenticação em dev
    });
}

// Agendar jobs recorrentes
RecurringJob.AddOrUpdate<VerificacaoInatividadeJob>(
    "verificacao-inatividade",
    job => job.ExecutarAsync(),
    "50 23 * * *",          // 23h50 diariamente (UTC)
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

RecurringJob.AddOrUpdate<DispararAlertaJob>(
    "disparar-alerta",
    job => job.ExecutarAsync(),
    "0 * * * *",            // a cada hora
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHangfireDashboard();

app.Run();

public partial class Program { }
