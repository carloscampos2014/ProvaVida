namespace ProvaVida.Dominio.Enums;

/// <summary>
/// Enum que categoriza os tipos de notificação que podem ser disparadas no sistema.
/// </summary>
public enum TipoNotificacao
{
    /// <summary>
    /// Lembrete enviado quando faltam 6 horas para o vencimento do prazo de 48h.
    /// </summary>
    Lembrete6h = 1,

    /// <summary>
    /// Lembrete enviado quando faltam 2 horas para o vencimento do prazo de 48h.
    /// </summary>
    Lembrete2h = 2,

    /// <summary>
    /// Notificação de emergência disparada quando o prazo de 48h é ultrapassado.
    /// Repetida a cada 6 horas até que um check-in seja realizado.
    /// </summary>
    Emergencia = 3
}
