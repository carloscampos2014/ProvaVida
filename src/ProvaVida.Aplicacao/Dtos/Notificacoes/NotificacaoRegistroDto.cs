namespace ProvaVida.Aplicacao.Dtos.Notificacoes;

/// <summary>
/// DTO para filtro de busca de notificações.
/// Entrada: critérios de busca (usuário, tipo, status).
/// </summary>
public class NotificacaoRegistroDto
{
    /// <summary>ID do usuário para filtrar.</summary>
    public Guid UsuarioId { get; set; }

    /// <summary>Apenas pendentes? (padrão: true).</summary>
    public bool ApenasNaoProcesadas { get; set; } = true;

    /// <summary>Quantidade máxima de resultados (padrão: 10).</summary>
    public int Limite { get; set; } = 10;
}
