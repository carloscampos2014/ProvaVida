namespace ProvaVida.Shared.Entities;

/// <summary>
/// Entidade de check-in de prova de vida do usuário.
/// </summary>
/// <remarks>
/// POCO puro mapeado por Dapper. Usado tanto pelo PostgreSQL (API/Admin) quanto pelo SQLite (Mobile).
/// A restrição de unicidade (usuario_id, data) é garantida no banco de dados.
/// </remarks>
public class Checkin
{
    /// <summary>Identificador único do check-in.</summary>
    public Guid Id { get; set; }

    /// <summary>Identificador do usuário que realizou o check-in.</summary>
    public Guid UsuarioId { get; set; }

    /// <summary>Data do check-in (sem horário — um por dia por usuário).</summary>
    public DateOnly Data { get; set; }

    /// <summary>Latitude da localização no momento do check-in.</summary>
    public double Latitude { get; set; }

    /// <summary>Longitude da localização no momento do check-in.</summary>
    public double Longitude { get; set; }

    /// <summary>Identificador único do aparelho que realizou o check-in.</summary>
    public string IdentificacaoAparelho { get; set; } = string.Empty;

    /// <summary>Indica se o check-in foi sincronizado com o servidor.</summary>
    public bool Sincronizado { get; set; }

    /// <summary>Data e hora de criação do registro (UTC).</summary>
    public DateTimeOffset CriadoEm { get; set; }
}
