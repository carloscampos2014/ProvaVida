namespace ProvaVida.Aplicacao.Dtos.CheckIns;

/// <summary>
/// DTO para resposta de check-in (confirmação ao usuário).
/// Saída: confirmação que check-in foi registrado.
/// </summary>
public class CheckInResumoDto
{
    /// <summary>ID do check-in registrado.</summary>
    public Guid Id { get; set; }

    /// <summary>ID do usuário.</summary>
    public Guid UsuarioId { get; set; }

    /// <summary>Data/hora do check-in.</summary>
    public DateTime DataCheckIn { get; set; }

    /// <summary>Próximo prazo de vencimento (sempre +48h).</summary>
    public DateTime DataProximoVencimento { get; set; }

    /// <summary>Mensagem amigável para o usuário.</summary>
    public string Mensagem { get; set; } = "✅ Check-in realizado com sucesso! Próximo prazo em 48 horas.";
}
