using System.Text;

namespace ProvaVida.Admin.Web.Middleware;

/// <summary>
/// Middleware de autenticação HTTP Basic para o painel Admin.
/// </summary>
/// <remarks>
/// Valida o header <c>Authorization: Basic &lt;base64&gt;</c> contra as variáveis de ambiente
/// <c>ADMIN_USUARIO</c> e <c>ADMIN_SENHA</c>. Paths de infraestrutura do Blazor Server
/// (<c>/_framework/</c>, <c>/_blazor</c>, <c>/favicon.ico</c>, <c>/not-found</c>)
/// são ignorados para não interromper a conexão SignalR ou o carregamento de assets estáticos.
/// </remarks>
public class BasicAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<BasicAuthMiddleware> _logger;

    /// <summary>
    /// Prefixos de path que devem ignorar a verificação de autenticação.
    /// </summary>
    private static readonly string[] BypassPaths =
    [
        "/_framework",
        "/_blazor",
        "/favicon.ico",
        "/not-found",
        "/_content"
    ];

    /// <summary>
    /// Inicializa o middleware com o delegado do próximo passo e o logger.
    /// </summary>
    /// <param name="next">Próximo delegado na pipeline HTTP.</param>
    /// <param name="logger">Logger para registrar tentativas inválidas de autenticação.</param>
    public BasicAuthMiddleware(RequestDelegate next, ILogger<BasicAuthMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Processa o request HTTP, validando as credenciais Basic Auth quando necessário.
    /// </summary>
    /// <param name="context">Contexto HTTP do request atual.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Bypassa paths de infraestrutura do Blazor para não quebrar SignalR e assets estáticos
        if (ShouldBypass(path))
        {
            await _next(context);
            return;
        }

        if (!TryAuthenticate(context))
        {
            _logger.LogWarning(
                "Tentativa de acesso não autorizado ao Admin. Path: {Path}, IP: {IP}",
                path,
                context.Connection.RemoteIpAddress);

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.Headers.WWWAuthenticate = "Basic realm=\"ProvaVida Admin\"";
            await context.Response.WriteAsync("401 - Não autorizado");
            return;
        }

        await _next(context);
    }

    private static bool ShouldBypass(string path)
    {
        foreach (var bypass in BypassPaths)
        {
            if (path.StartsWith(bypass, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static bool TryAuthenticate(HttpContext context)
    {
        var adminUsuario = Environment.GetEnvironmentVariable("ADMIN_USUARIO");
        var adminSenha = Environment.GetEnvironmentVariable("ADMIN_SENHA");

        // Se as env vars não estiverem configuradas, bloqueia acesso
        if (string.IsNullOrWhiteSpace(adminUsuario) || string.IsNullOrWhiteSpace(adminSenha))
            return false;

        var authHeader = context.Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            return false;

        string encoded;
        try
        {
            var base64 = authHeader["Basic ".Length..].Trim();
            encoded = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        }
        catch
        {
            return false;
        }

        var separatorIndex = encoded.IndexOf(':');
        if (separatorIndex < 0)
            return false;

        var usuario = encoded[..separatorIndex];
        var senha = encoded[(separatorIndex + 1)..];

        return string.Equals(usuario, adminUsuario, StringComparison.Ordinal)
            && string.Equals(senha, adminSenha, StringComparison.Ordinal);
    }
}
