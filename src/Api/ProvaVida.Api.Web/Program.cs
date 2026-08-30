using ProvaVida.Api.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

// Executa as migrations do banco de dados antes de aceitar requests.
// Em produção, a connection string vem da variável de ambiente DB_CONNECTION_STRING.
// Em desenvolvimento, vem do appsettings.Development.json (não versionado).
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
    ?? throw new InvalidOperationException(
        "Connection string não configurada. " +
        "Defina 'ConnectionStrings:DefaultConnection' no appsettings ou a variável de ambiente 'DB_CONNECTION_STRING'.");

var migrationLogger = app.Services.GetRequiredService<ILogger<DatabaseMigrator>>();
var migrator = new DatabaseMigrator(connectionString, migrationLogger);
migrator.Migrate();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
