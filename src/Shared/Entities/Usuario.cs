namespace ProvaVida.Shared.Entities;

/// <summary>
/// Entidade de usuário do sistema ProvaVida.
/// </summary>
/// <remarks>
/// POCO puro mapeado por Dapper. Usado tanto pelo PostgreSQL (API/Admin) quanto pelo SQLite (Mobile).
/// Os nomes das propriedades correspondem às colunas via snake_case mapping configurado no startup.
/// </remarks>
public class Usuario
{
    /// <summary>Identificador único do usuário.</summary>
    public Guid Id { get; set; }

    /// <summary>Nome completo do usuário.</summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>Endereço de e-mail do usuário (único no sistema).</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Número de WhatsApp do usuário com DDD.</summary>
    public string Whatsapp { get; set; } = string.Empty;

    /// <summary>Hash SHA-256 da senha, gerado no app mobile antes do envio.</summary>
    public string SenhaHash { get; set; } = string.Empty;

    /// <summary>Nome do contato de emergência.</summary>
    public string ContatoEmergenciaNome { get; set; } = string.Empty;

    /// <summary>E-mail do contato de emergência.</summary>
    public string ContatoEmergenciaEmail { get; set; } = string.Empty;

    /// <summary>WhatsApp do contato de emergência com DDD.</summary>
    public string ContatoEmergenciaWhatsapp { get; set; } = string.Empty;

    /// <summary>Data e hora de criação do registro (UTC).</summary>
    public DateTimeOffset CriadoEm { get; set; }

    /// <summary>Data e hora da última atualização do registro (UTC).</summary>
    public DateTimeOffset AtualizadoEm { get; set; }
}
