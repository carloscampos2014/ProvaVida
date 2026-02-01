namespace ProvaVida.API.Middleware;

/// <summary>
/// Extensões para registrar middlewares no pipeline.
/// Padrão: UseNomeMiddleware().
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Registra o GlobalExceptionMiddleware.
    /// DEVE SER CHAMADO ANTES de UseRouting().
    /// 
    /// Uso: app.UseGlobalExceptionMiddleware();
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionMiddleware(
        this IApplicationBuilder app)
    {
        if (app == null)
            throw new ArgumentNullException(nameof(app));

        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
