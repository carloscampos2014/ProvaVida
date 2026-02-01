namespace ProvaVida.Dominio.Enums;

/// <summary>
/// Enum que categoriza os tipos de notificação que podem ser disparadas no sistema.
/// 
/// - LembreteUsuario: Notificações simples para o próprio usuário (4h antes + no vencimento).
/// - EmergenciaContatos: Notificações detalhadas para contatos de emergência (a cada 6h por 48h após vencimento).
/// </summary>
public enum TipoNotificacao
{
    /// <summary>
    /// Lembrete enviado para o próprio usuário: 4 horas antes do vencimento e no momento do vencimento.
    /// </summary>
    LembreteUsuario = 1,

    /// <summary>
    /// Notificação de emergência enviada para contatos de emergência.
    /// Disparada 24h após o vencimento (quando o usuário não fez check-in por 1 dia).
    /// Reenvios automáticos a cada 6 horas por até 48 horas.
    /// </summary>
    EmergenciaContatos = 2,
}
