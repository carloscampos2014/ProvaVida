using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace ProvaVida.Api.Extensions;

public static class RateLimitingExtensions
{
    // Nomes das políticas — usados nos atributos [EnableRateLimiting]
    public const string PolicyLogin    = "login";
    public const string PolicyCadastro = "cadastro";
    public const string PolicyCheckIn  = "checkin";
    public const string PolicyGeral    = "geral";

    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(opts =>
        {
            // Resposta padrão para 429
            opts.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            opts.OnRejected = async (ctx, ct) =>
            {
                ctx.HttpContext.Response.Headers.RetryAfter = "60";
                ctx.HttpContext.Response.ContentType = "application/json";
                await ctx.HttpContext.Response.WriteAsync(
                    "{\"error\":\"Muitas requisições. Aguarde e tente novamente.\"}", ct);
            };

            // Login — 10 req/min por IP
            opts.AddPolicy(PolicyLogin, ctx =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ObterIp(ctx),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit      = 10,
                        Window           = TimeSpan.FromMinutes(1),
                        QueueLimit       = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    }));

            // Cadastro — 5 req/hora por IP
            opts.AddPolicy(PolicyCadastro, ctx =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: $"cadastro:{ObterIp(ctx)}",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit      = 5,
                        Window           = TimeSpan.FromHours(1),
                        QueueLimit       = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    }));

            // Check-in — 10 req/hora por usuário autenticado
            opts.AddPolicy(PolicyCheckIn, ctx =>
            {
                var userId = ctx.User?.FindFirst("sub")?.Value ?? ObterIp(ctx);
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: $"checkin:{userId}",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit      = 10,
                        Window           = TimeSpan.FromHours(1),
                        QueueLimit       = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    });
            });

            // Geral — 60 req/min por IP (todos os outros endpoints)
            opts.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
            {
                // Exclui /admin e /hangfire
                var path = ctx.Request.Path.Value ?? string.Empty;
                if (path.StartsWith("/admin", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith("/hangfire", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith("/health", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith("/scalar", StringComparison.OrdinalIgnoreCase))
                {
                    return RateLimitPartition.GetNoLimiter("exempt");
                }

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: $"geral:{ObterIp(ctx)}",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit      = 60,
                        Window           = TimeSpan.FromMinutes(1),
                        QueueLimit       = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    });
            });
        });

        return services;
    }

    /// <summary>
    /// Lê o IP real do cliente — prioriza o header CF-Connecting-IP do Cloudflare.
    /// Evita que o IP do proxy/Cloudflare bloqueie todos os usuários.
    /// </summary>
    private static string ObterIp(HttpContext ctx)
    {
        if (ctx.Request.Headers.TryGetValue("CF-Connecting-IP", out var cfIp) &&
            !string.IsNullOrWhiteSpace(cfIp))
            return cfIp.ToString();

        if (ctx.Request.Headers.TryGetValue("X-Forwarded-For", out var xff) &&
            !string.IsNullOrWhiteSpace(xff))
        {
            var primeiro = xff.ToString().Split(',')[0].Trim();
            if (!string.IsNullOrEmpty(primeiro))
                return primeiro;
        }

        return ctx.Connection.RemoteIpAddress?.ToString()
               ?? IPAddress.Loopback.ToString();
    }
}
