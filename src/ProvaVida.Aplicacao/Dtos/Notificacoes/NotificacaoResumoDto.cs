using ProvaVida.Dominio.Enums;

namespace ProvaVida.Aplicacao.Dtos.Notificacoes;

/// <summary>
/// DTO para resposta de notificação.
/// Saída: resumo para cliente/contato.
/// </summary>
public class NotificacaoResumoDto
{
    /// <summary>ID da notificação.</summary>
    public Guid Id { get; set; }

    /// <summary>Tipo (Lembrete ou Emergencia).</summary>
    public TipoNotificacao TipoNotificacao { get; set; }

    /// <summary>Meio (Email, WhatsApp).</summary>
    public MeioNotificacao MeioNotificacao { get; set; }

    /// <summary>Status (Pendente, Enviada, Erro).</summary>
    public StatusNotificacao Status { get; set; }

    /// <summary>Data de criação.</summary>
    public DateTime DataCriacao { get; set; }

    /// <summary>Próximo reenvio agendado (se emergência).</summary>
    public DateTime? DataProximoReenvio { get; set; }
}
