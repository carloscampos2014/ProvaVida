namespace ProvaVida.API.Respostas;

/// <summary>
/// Resposta padronizada para erros.
/// Retornada por GlobalExceptionMiddleware.
/// </summary>
public class ErrorResponse
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

    /// <example>
    /// {
    ///   "titulo": "Email Já Registrado",
    ///   "mensagem": "Email 'joao@example.com' já está registrado no sistema.",
    ///   "statusCode": 409,
    ///   "timestamp": "2026-01-31T14:35:00Z",
    ///   "detalhes": null (em produção) ou { "type": "UsuarioJaExisteException", "stackTrace": "..." } (dev)
    /// }
    /// </example>
}
