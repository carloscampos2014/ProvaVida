using ProvaVida.Aplicacao.Exceções;
using ProvaVida.Dominio.Exceções;
using System.Text.Json;

namespace ProvaVida.API.Middleware;

/// <summary>
/// Middleware global para capturar e tratar TODAS as exceções.
/// 
/// Responsabilidades:
/// - Mapear exceções para status codes HTTP corretos
/// - Retornar JSON padronizado
/// - NÃO expor stack traces em produção
/// - Logar erros para diagnóstico
/// 
/// ORDEM: Deve ser registrado ANTES do routing (UseRouting).
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _proximoMiddleware;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate proximoMiddleware, ILogger<GlobalExceptionMiddleware> logger)
    {
        _proximoMiddleware = proximoMiddleware ?? throw new ArgumentNullException(nameof(proximoMiddleware));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Invoca o middleware.
    /// Envolve todo o pipeline em um try-catch.
    /// </summary>
    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            // Chamar próximo middleware
            await _proximoMiddleware(contexto);
        }
        catch (Exception excecao)
        {
            // Capturou exceção não tratada
            _logger.LogError(excecao, "❌ Exceção não tratada no middleware global: {Mensagem}", excecao.Message);

            // Tratar e retornar resposta
            await TratarExcecaoAsync(contexto, excecao);
        }
    }

    /// <summary>
    /// Trata a exceção e retorna resposta HTTP padronizada.
    /// 
    /// Mapeamento:
    /// - UsuarioJaExisteException → 409 Conflict
    /// - UsuarioNaoEncontradoException → 404 Not Found
    /// - SenhaInvalidaException → 401 Unauthorized
    /// - UsuarioInativoException → 403 Forbidden
    /// - UsuarioInvalidoException (Domínio) → 422 Unprocessable Entity
    /// - AplicacaoException (genérica) → 400 Bad Request
    /// - Exception (qualquer outra) → 500 Internal Server Error
    /// </summary>
    private Task TratarExcecaoAsync(HttpContext contexto, Exception excecao)
    {
        // ✅ Definir response content type
        contexto.Response.ContentType = "application/json";

        // 🔍 Determinar status code e mensagem
        var (statusCode, titulo, mensagem) = excecao switch
        {
            // 🟠 Exceções de Aplicação (400+)
            UsuarioJaExisteException ex => (
                StatusCodes.Status409Conflict,
                "Email Já Registrado",
                ex.Message
            ),

            UsuarioNaoEncontradoException ex => (
                StatusCodes.Status404NotFound,
                "Usuário Não Encontrado",
                ex.Message
            ),

            SenhaInvalidaException ex => (
                StatusCodes.Status401Unauthorized,
                "Autenticação Falhou",
                ex.Message
            ),

            UsuarioInativoException ex => (
                StatusCodes.Status403Forbidden,
                "Usuário Inativo",
                ex.Message
            ),

            ContatoNaoEncontradoException ex => (
                StatusCodes.Status404NotFound,
                "Contato Não Encontrado",
                ex.Message
            ),

            AplicacaoException ex => (
                StatusCodes.Status400BadRequest,
                "Erro na Aplicação",
                ex.Message
            ),

            // 🔴 Exceções do Domínio
            UsuarioInvalidoException ex => (
                StatusCodes.Status422UnprocessableEntity,
                "Dados Inválidos",
                ex.Message
            ),

            // ⚫ Exceção genérica (500)
            _ => (
                StatusCodes.Status500InternalServerError,
                "Erro Interno do Servidor",
                "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde."
            )
        };

        contexto.Response.StatusCode = statusCode;

        // 📋 Construir resposta padronizada
        var resposta = new ErrorResponse
        {
            Titulo = titulo,
            Mensagem = mensagem,
            StatusCode = statusCode,
            Timestamp = DateTime.UtcNow,
            
            // ⚠️ Apenas em desenvolvimento: incluir detalhes técnicos
            Detalhes = contexto.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() == true
                ? new
                {
                    excecao.GetType().Name,
                    StackTrace = excecao.StackTrace
                }
                : null
        };

        // 🔐 Logar o erro completo (sempre, mesmo em produção)
        _logger.LogError(
            "⚠️ Exceção capturada: {Tipo} | StatusCode: {StatusCode} | Mensagem: {Mensagem}",
            excecao.GetType().Name,
            statusCode,
            excecao.Message
        );

        // 📤 Serializar e retornar resposta
        var json = JsonSerializer.Serialize(
            resposta,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );

        return contexto.Response.WriteAsync(json);
    }
}

/// <summary>
/// Resposta padronizada para erros.
/// Retornada por GlobalExceptionMiddleware.
/// </summary>
internal class ErrorResponse
{
    /// <summary>Título breve do erro.</summary>
    public string Titulo { get; set; } = null!;

    /// <summary>Mensagem descritiva.</summary>
    public string Mensagem { get; set; } = null!;

    /// <summary>Status code HTTP.</summary>
    public int StatusCode { get; set; }

    /// <summary>Timestamp UTC do erro.</summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Detalhes técnicos (apenas em desenvolvimento).
    /// Em produção: null.
    /// </summary>
    public object? Detalhes { get; set; }
}
