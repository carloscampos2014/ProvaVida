namespace ProvaVida.Dominio.Enums;

/// <summary>
/// Enum que representa os possíveis estados de um usuário no sistema de monitoramento.
/// </summary>
public enum StatusUsuario
{
    /// <summary>
    /// Usuário está ativo e dentro do prazo de 48 horas.
    /// </summary>
    Ativo = 1,

    /// <summary>
    /// Usuário está com check-in atrasado (passou do prazo de 48 horas).
    /// </summary>
    EmAtraso = 2,

    /// <summary>
    /// Situação crítica: contatos de emergência estão sendo notificados.
    /// </summary>
    AlertaCritico = 3,

    /// <summary>
    /// Usuário desativou o monitoramento.
    /// </summary>
    Inativo = 4
}
