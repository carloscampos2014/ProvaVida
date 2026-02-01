using ProvaVida.Aplicacao.Dtos.Notificacoes;
using ProvaVida.Dominio.Entidades;

namespace ProvaVida.Aplicacao.Mapeadores;

/// <summary>
/// Mapeador manual entre Notificacao (Entidade) e DTOs.
/// 
/// Notificacoes são read-only (cliente não cria, apenas consulta).
/// Por isso não há método ParaDominio (apenas ParaResumoDto).
/// </summary>
public static class NotificacaoMapeador
{
    /// <summary>
    /// Mapeia Notificacao (Entidade) para NotificacaoResumoDto (resposta).
    /// 
    /// Exibe ao usuário/contato:
    /// - Tipo (Lembrete, Emergencia)
    /// - Meio (Email, WhatsApp)
    /// - Status (Pendente, Enviada, Erro)
    /// - Próximo reenvio (se aplicável)
    /// </summary>
    public static NotificacaoResumoDto ParaResumoDto(this Notificacao notificacao)
    {
        if (notificacao == null)
            throw new ArgumentNullException(nameof(notificacao), "Notificação não pode ser nula.");

        return new NotificacaoResumoDto
        {
            Id = notificacao.Id,
            TipoNotificacao = notificacao.TipoNotificacao,
            MeioNotificacao = notificacao.MeioNotificacao,
            Status = notificacao.Status,
            DataCriacao = notificacao.DataCriacao,
            DataProximoReenvio = notificacao.DataProximoReenvio
        };
    }

    /// <summary>
    /// Converte lista de Notificacoes para DTOs.
    /// Utilitário para respostas em massa.
    /// </summary>
    public static List<NotificacaoResumoDto> ParaResumosDtos(
        this IEnumerable<Notificacao> notificacoes)
    {
        if (notificacoes == null)
            throw new ArgumentNullException(nameof(notificacoes));

        return notificacoes
            .Select(n => n.ParaResumoDto())
            .ToList();
    }
}
