using Hangfire.Dashboard;
using System.Net.Http.Headers;
using System.Text;

namespace ProvaVida.Api.Filters;

/// <summary>
/// Filtro de autenticação Basic para o dashboard do Hangfire.
/// Usa as mesmas credenciais do painel Admin (Admin:Usuario e Admin:Senha).
/// </summary>
public sealed class HangfireBasicAuthFilter : IDashboardAuthorizationFilter
{
    private readonly IConfiguration _config;

    public HangfireBasicAuthFilter(IConfiguration config)
    {
        _config = config;
    }

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        if (!httpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            Desafiar(httpContext);
            return false;
        }

        try
        {
            var header = AuthenticationHeaderValue.Parse(authHeader!);
            if (!string.Equals(header.Scheme, "Basic", StringComparison.OrdinalIgnoreCase))
            {
                Desafiar(httpContext);
                return false;
            }

            var credenciais = Encoding.UTF8.GetString(
                Convert.FromBase64String(header.Parameter ?? string.Empty));

            var separador = credenciais.IndexOf(':', StringComparison.Ordinal);
            if (separador < 0) { Desafiar(httpContext); return false; }

            var usuario = credenciais[..separador];
            var senha   = credenciais[(separador + 1)..];

            var usuarioEsperado = _config["Admin:Usuario"] ?? "admin";
            var senhaEsperada   = _config["Admin:Senha"]   ?? string.Empty;

            if (string.IsNullOrEmpty(senhaEsperada)) { Desafiar(httpContext); return false; }

            if (string.Equals(usuario, usuarioEsperado, StringComparison.Ordinal)
                && string.Equals(senha, senhaEsperada, StringComparison.Ordinal))
                return true;

            Desafiar(httpContext);
            return false;
        }
        catch
        {
            Desafiar(httpContext);
            return false;
        }
    }

    private static void Desafiar(HttpContext ctx)
    {
        ctx.Response.StatusCode = 401;
        ctx.Response.Headers.WWWAuthenticate = "Basic realm=\"ProvaVida Hangfire\"";
    }
}
