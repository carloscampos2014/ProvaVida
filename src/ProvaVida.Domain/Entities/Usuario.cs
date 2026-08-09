namespace ProvaVida.Domain.Entities;

public class Usuario
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string WhatsApp { get; private set; } = string.Empty;
    public string SenhaHash { get; private set; } = string.Empty;
    public bool Ativo { get; private set; }

    // Contato de emergência (desnormalizado — um único contato por usuário)
    public string ContatoEmergenciaNome { get; private set; } = string.Empty;
    public string ContatoEmergenciaEmail { get; private set; } = string.Empty;
    public string ContatoEmergenciaWhatsApp { get; private set; } = string.Empty;

    public DateTime CriadoEm { get; private set; }
    public DateTime AtualizadoEm { get; private set; }

    // EF Core
    protected Usuario() { }

    public static Usuario Criar(
        string nome,
        string email,
        string whatsApp,
        string senhaHash,
        string contatoEmergenciaNome,
        string contatoEmergenciaEmail,
        string contatoEmergenciaWhatsApp)
    {
        return new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = nome.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            WhatsApp = whatsApp.Trim(),
            SenhaHash = senhaHash,
            Ativo = true,
            ContatoEmergenciaNome = contatoEmergenciaNome.Trim(),
            ContatoEmergenciaEmail = contatoEmergenciaEmail.Trim().ToLowerInvariant(),
            ContatoEmergenciaWhatsApp = contatoEmergenciaWhatsApp.Trim(),
            CriadoEm = DateTime.UtcNow,
            AtualizadoEm = DateTime.UtcNow
        };
    }

    public void AtualizarDados(
        string nome,
        string whatsApp,
        string contatoEmergenciaNome,
        string contatoEmergenciaEmail,
        string contatoEmergenciaWhatsApp)
    {
        Nome = nome.Trim();
        WhatsApp = whatsApp.Trim();
        ContatoEmergenciaNome = contatoEmergenciaNome.Trim();
        ContatoEmergenciaEmail = contatoEmergenciaEmail.Trim().ToLowerInvariant();
        ContatoEmergenciaWhatsApp = contatoEmergenciaWhatsApp.Trim();
        AtualizadoEm = DateTime.UtcNow;
    }

    /// <summary>
    /// Anonimiza os dados pessoais conforme LGPD.
    /// O registro é mantido para integridade referencial, mas sem dados identificáveis.
    /// </summary>
    public void Anonimizar()
    {
        var anonId = Id.ToString("N")[..8];
        Nome = $"[removido-{anonId}]";
        Email = $"removido-{anonId}@anonimizado.invalid";
        WhatsApp = "[removido]";
        SenhaHash = string.Empty;
        ContatoEmergenciaNome = "[removido]";
        ContatoEmergenciaEmail = $"removido-contato-{anonId}@anonimizado.invalid";
        ContatoEmergenciaWhatsApp = "[removido]";
        Ativo = false;
        AtualizadoEm = DateTime.UtcNow;
    }
}
