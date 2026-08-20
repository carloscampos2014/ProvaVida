using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProvaVida.Api.Filters;

/// <summary>
/// Exige o header X-Admin-Key com a chave configurada em Admin:ApiKey.
/// Protege todos os endpoints do AdminController contra acesso não autorizado.
/// </summary>
public class AdminApiKeyFilter : IActionFilter
{
    private const string HeaderName = "X-Admin-Key";
    private readonly string _apiKey;

    public AdminApiKeyFilter(IConfiguration configuration)
    {
        _apiKey = configuration["Admin:ApiKey"]
            ?? throw new InvalidOperationException("Admin:ApiKey não configurada.");
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var key)
            || key != _apiKey)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                error = "Chave de admin inválida ou ausente."
            });
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
