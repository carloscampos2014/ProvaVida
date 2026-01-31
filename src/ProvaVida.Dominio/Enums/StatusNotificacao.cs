namespace ProvaVida.Dominio.Enums;

/// <summary>
/// Enum que representa o estado atual de uma notificação no sistema.
/// </summary>
public enum StatusNotificacao
{
    /// <summary>
    /// Notificação foi criada mas ainda não foi enviada.
    /// </summary>
    Pendente = 1,

    /// <summary>
    /// Notificação foi enviada com sucesso para o destinatário.
    /// </summary>
    Enviada = 2,

    /// <summary>
    /// Houve erro ao tentar enviar a notificação.
    /// </summary>
    Erro = 3,

    /// <summary>
    /// Notificação foi cancelada (ex: usuário fez check-in antes do envio).
    /// </summary>
    Cancelada = 4
}
