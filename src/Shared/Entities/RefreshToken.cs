namespace ProvaVida.Shared.Entities;

/// <summary>
/// Entidade de refresh token de autenticação do usuário.
/// </summary>
/// <remarks>
/// POCO puro mapeado por Dapper. Armazena tokens de renovação de sessão no PostgreSQL.
/// A revogação é lógica — tokens expirados ou revogados são mantidos para auditoria.
/// </remarks>
public class RefreshToken
{
    /// <summary>Identificador único do refresh token.</summary>
    public Guid Id { get; set; }

    /// <summary>Identificador do usuário ao qual o token pertence.</summary>
    public Guid UsuarioId { get; set; }

    /// <summary>Valor do token (string Base64 de 64 bytes, criptograficamente aleatório).</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Data e hora de expiração do token (UTC).</summary>
    public DateTimeOffset ExpiraEm { get; set; }

    /// <summary>Indica se o token foi revogado.</summary>
    public bool Revogado { get; set; }

    /// <summary>Data e hora de criação do registro (UTC).</summary>
    public DateTimeOffset CriadoEm { get; set; }

    /// <summary>Data e hora em que o token foi revogado (UTC). Nulo se ainda não revogado.</summary>
    public DateTimeOffset? RevogadoEm { get; set; }
}
