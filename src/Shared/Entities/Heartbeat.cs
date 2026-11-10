namespace ProvaVida.Shared.Entities;

/// <summary>
/// Entidade de heartbeat de prova de vida do usuário.
/// </summary>
/// <remarks>
/// POCO puro mapeado por Dapper. Usado tanto pelo PostgreSQL (API/Admin) quanto pelo SQLite (Mobile).
/// Representa a confirmação de que o usuário ainda está vivo e ativo no sistema.
/// </remarks>
public class Heartbeat
{
    /// <summary>Identificador único do heartbeat.</summary>
    public Guid Id { get; set; }

    /// <summary>Identificador do usuário que enviou o heartbeat.</summary>
    public Guid UsuarioId { get; set; }

    /// <summary>Data e hora em que o heartbeat foi recebido pelo servidor (UTC).</summary>
    public DateTime RecebidoEm { get; set; }

    /// <summary>Data e hora de criação do registro (UTC).</summary>
    public DateTime CriadoEm { get; set; }
}