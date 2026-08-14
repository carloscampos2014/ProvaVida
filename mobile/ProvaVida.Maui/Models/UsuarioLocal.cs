namespace ProvaVida.Maui.Models;

/// <summary>
/// Dados do usuário logado armazenados localmente via Preferences.
/// Usados para exibição rápida sem chamada de rede.
/// </summary>
public class UsuarioLocal
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string WhatsApp { get; set; } = string.Empty;
    public string ContatoEmergenciaNome { get; set; } = string.Empty;
    public string ContatoEmergenciaEmail { get; set; } = string.Empty;
    public string ContatoEmergenciaWhatsApp { get; set; } = string.Empty;
}
