namespace ProvaVida.Aplicacao.Dtos.ContatosEmergencia;

/// <summary>
/// DTO para registrar um novo contato de emergência.
/// Entrada: nome, email, whatsapp, prioridade.
/// </summary>
public class ContatoRegistroDto
{
    /// <summary>Nome do contato.</summary>
    public string Nome { get; set; } = null!;

    /// <summary>Email do contato.</summary>
    public string Email { get; set; } = null!;

    /// <summary>WhatsApp do contato (será validado).</summary>
    public string WhatsApp { get; set; } = null!;

    /// <summary>Prioridade de notificação (1-10, padrão: 1).</summary>
    public int? Prioridade { get; set; }
}
