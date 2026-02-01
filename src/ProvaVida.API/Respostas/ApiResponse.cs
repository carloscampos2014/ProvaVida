namespace ProvaVida.API.Respostas;

/// <summary>
/// Resposta padronizada para sucesso.
/// Envelope genérico que encapsula dados e metadados.
/// </summary>
/// <typeparam name="T">Tipo dos dados retornados.</typeparam>
public class ApiResponse<T>
{
    /// <summary>Dados da resposta.</summary>
    public T Dados { get; set; } = default!;

    /// <summary>Mensagem de sucesso (opcional).</summary>
    public string? Mensagem { get; set; }

    /// <summary>Timestamp UTC da resposta.</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <example>
    /// {
    ///   "dados": {
    ///     "id": "550e8400-e29b-41d4-a716-446655440000",
    ///     "nome": "João Silva",
    ///     "email": "joao@example.com"
    ///   },
    ///   "mensagem": "✅ Usuário registrado com sucesso!",
    ///   "timestamp": "2026-01-31T14:35:00Z"
    /// }
    /// </example>
}
