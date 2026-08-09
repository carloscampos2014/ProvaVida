using FluentValidation;
using ProvaVida.Application.Common;
using ProvaVida.Infrastructure.Persistence;

namespace ProvaVida.Api.Extensions;

public static class WebApplicationExtensions
{
    /// <summary>
    /// Aplica migrations DbUp pendentes na inicialização.
    /// </summary>
    public static void ApplyMigrations(this WebApplication app)
    {
        var migrator = app.Services.GetRequiredService<DatabaseMigrator>();
        migrator.MigrateUp();
    }

    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            try
            {
                await next();
            }
            catch (ValidationException ex)
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                var errors = ex.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage });
                await context.Response.WriteAsJsonAsync(new { errors });
            }
            catch (AppException ex)
            {
                context.Response.StatusCode = ex.StatusCode;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                var logger = context.RequestServices
                    .GetRequiredService<ILogger<WebApplication>>();
                logger.LogError(ex, "Erro inesperado.");
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { error = "Erro interno no servidor." });
            }
        });

        return app;
    }
}
