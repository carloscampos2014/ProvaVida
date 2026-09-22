using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using ProvaVida.Api.Application.Services;
using ProvaVida.Api.Domain.Interfaces;
using ProvaVida.Api.Infrastructure;
using ProvaVida.Api.Infrastructure.Data;
using ProvaVida.Api.Infrastructure.Repositories;
using ProvaVida.Api.Infrastructure.Services;
using ProvaVida.Shared.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Connection string: env var tem precedência sobre appsettings
var connectionString =
    Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string não configurada. " +
        "Defina a variável de ambiente 'DB_CONNECTION_STRING' ou 'ConnectionStrings:DefaultConnection' no appsettings.");

// IDbConnectionFactory como Singleton (stateless, só guarda a connection string)
builder.Services.AddSingleton<IDbConnectionFactory>(_ => new PostgresConnectionFactory(connectionString));

// Repositórios
builder.Services.AddScoped<IUsuarioRepository, PostgresUsuarioRepository>();
builder.Services.AddScoped<ICheckinRepository, PostgresCheckinRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, PostgresRefreshTokenRepository>();

// Serviços de autenticação
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthApplicationService, AuthApplicationService>();

// JWT Bearer — lê de env var com fallback para Jwt:Secret na configuração
var jwtSecret =
    Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException(
        "Segredo JWT não configurado. " +
        "Defina a variável de ambiente 'JWT_SECRET' ou 'Jwt:Secret' no appsettings.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();

// Controllers
builder.Services.AddControllers();

// OpenAPI + Scalar
builder.Services.AddOpenApi();

var app = builder.Build();

// DbUp migrations — executadas antes de aceitar requests
var migrationLogger = app.Services.GetRequiredService<ILogger<DatabaseMigrator>>();
var migrator = new DatabaseMigrator(connectionString, migrationLogger);
migrator.Migrate();

// Scalar UI
app.MapScalarApiReference(options =>
{
    options.WithTitle("ProvaVida API");
    options.WithEndpointPrefix("/scalar/{documentName}");
});
app.MapOpenApi();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
