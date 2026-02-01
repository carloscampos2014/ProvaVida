namespace ProvaVida.Aplicacao.Dtos.ContatosEmergencia;

/// <summary>
/// DTO para resposta de contato de emergência.
/// Saída: resumo do contato cadastrado.
/// </summary>
public class ContatoResumoDto
{
    /// <summary>ID do contato.</summary>
    public Guid Id { get; set; }

    /// <summary>ID do usuário proprietário.</summary>
    public Guid UsuarioId { get; set; }

    /// <summary>Nome do contato.</summary>
    public string Nome { get; set; } = null!;

    /// <summary>Email do contato.</summary>
    public string Email { get; set; } = null!;

    /// <summary>WhatsApp do contato.</summary>
    public string WhatsApp { get; set; } = null!;

    /// <summary>Prioridade de notificação.</summary>
    public int Prioridade { get; set; }

    /// <summary>Se contato está ativo.</summary>
    public bool Ativo { get; set; }

    /// <summary>Data de criação.</summary>
    public DateTime DataCriacao { get; set; }
}
