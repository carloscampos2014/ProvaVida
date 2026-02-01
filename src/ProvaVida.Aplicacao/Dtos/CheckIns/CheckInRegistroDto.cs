namespace ProvaVida.Aplicacao.Dtos.CheckIns;

/// <summary>
/// DTO para registrar um novo check-in.
/// Entrada: apenas ID do usuário + localização opcional.
/// </summary>
public class CheckInRegistroDto
{
    /// <summary>ID do usuário que está fazendo check-in.</summary>
    public Guid UsuarioId { get; set; }

    /// <summary>Localização GPS opcional (formato: "-23.550519,-46.633309").</summary>
    public string? Localizacao { get; set; }
}
