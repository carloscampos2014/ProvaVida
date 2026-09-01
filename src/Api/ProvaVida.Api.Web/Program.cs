using System.Data;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Scalar.AspNetCore;
using ProvaVida.Api.Infrastructure.Data;
using ProvaVida.Api.Infrastructure.Repositories;
using ProvaVida.Shared.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Connection string: env var tem precedência sobre appsettings
var connectionString =
    Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string não configurada. " +
        "Defina a variável de ambiente 'DB_CONNECTION_STRING' ou 'ConnectionStrings:DefaultConnection' no appsettings.");

// IDbConnection como Scoped
builder.Services.AddScoped<IDbConnection>(_ => new NpgsqlConnection(connectionString));

// Repositórios
builder.Services.AddScoped<IUsuarioRepository, PostgresUsuarioRepository>();
builder.Services.AddScoped<ICheckinRepository, PostgresCheckinRepository>();

// JWT Bearer
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? throw new InvalidOperationException("Variável de ambiente 'JWT_SECRET' não configurada.");

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

app.Run();
