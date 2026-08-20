using ProvaVida.Application.Interfaces;

namespace ProvaVida.Api.Middleware;

/// <summary>
/// Middleware que protege endpoints de autenticação contra brute force.
/// Verifica bloqueio antes de processar e registra tentativas após resposta 401.
/// </summary>
public class BruteForceMiddleware
{
    private readonly RequestDelegate _next;

    // Limites por endpoint
    private static readonly Dictionary<string, int> Limites = new(StringComparer.OrdinalIgnoreCase)
    {
        ["/auth/login"]    = 5,
        ["/auth/cadastro"] = 3,
        ["/auth/refresh"]  = 10
    };

    public BruteForceMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IBruteForceService bruteForce)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Só monitora endpoints de autenticação
        var limite = Limites.FirstOrDefault(
            l => path.EndsWith(l.Key, StringComparison.OrdinalIgnoreCase));

        if (limite.Key is null)
        {
            await _next(context);
            return;
        }

        var ip = ObterIp(context);

        // Verifica se IP está bloqueado
        var expiraEm = await bruteForce.ObterBloqueioAsync(ip);
        if (expiraEm.HasValue)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";
            var liberaEm = expiraEm.Value.ToString("dd/MM/yyyy HH:mm") + " UTC";
            await context.Response.WriteAsJsonAsync(new
            {
                error   = $"Muitas tentativas. Acesso bloqueado até {liberaEm}.",
                expiraEm = expiraEm.Value
            });
            return;
        }

        // Processa a requisição
        await _next(context);

        // Registra tentativa apenas em caso de falha de autenticação (401)
        if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
        {
            await bruteForce.RegistrarTentativaAsync(ip, limite.Key, limite.Value);
        }
    }

    private static string ObterIp(HttpContext context)
    {
        // Respeita header do Cloudflare/Nginx
        var cfIp = context.Request.Headers["CF-Connecting-IP"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(cfIp)) return cfIp;

        var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwarded))
            return forwarded.Split(',')[0].Trim();

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
