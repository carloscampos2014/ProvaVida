using ProvaVida.Maui.Models;

namespace ProvaVida.Maui.Storage;

/// <summary>
/// Persiste dados básicos do usuário logado via Preferences (não sensível).
/// </summary>
public class UsuarioStorage : IUsuarioStorage
{
    private const string KeyNome = "usuario_nome";
    private const string KeyEmail = "usuario_email";
    private const string KeyWhatsApp = "usuario_whatsapp";
    private const string KeyContatoNome = "contato_nome";
    private const string KeyContatoEmail = "contato_email";
    private const string KeyContatoWhatsApp = "contato_whatsapp";

    public void Salvar(UsuarioLocal usuario)
    {
        Preferences.Default.Set(KeyNome, usuario.Nome);
        Preferences.Default.Set(KeyEmail, usuario.Email);
        Preferences.Default.Set(KeyWhatsApp, usuario.WhatsApp);
        Preferences.Default.Set(KeyContatoNome, usuario.ContatoEmergenciaNome);
        Preferences.Default.Set(KeyContatoEmail, usuario.ContatoEmergenciaEmail);
        Preferences.Default.Set(KeyContatoWhatsApp, usuario.ContatoEmergenciaWhatsApp);
    }

    public UsuarioLocal? Obter()
    {
        var nome = Preferences.Default.Get(KeyNome, string.Empty);
        if (string.IsNullOrEmpty(nome)) return null;

        return new UsuarioLocal
        {
            Nome = nome,
            Email = Preferences.Default.Get(KeyEmail, string.Empty),
            WhatsApp = Preferences.Default.Get(KeyWhatsApp, string.Empty),
            ContatoEmergenciaNome = Preferences.Default.Get(KeyContatoNome, string.Empty),
            ContatoEmergenciaEmail = Preferences.Default.Get(KeyContatoEmail, string.Empty),
            ContatoEmergenciaWhatsApp = Preferences.Default.Get(KeyContatoWhatsApp, string.Empty)
        };
    }

    public void Remover()
    {
        Preferences.Default.Remove(KeyNome);
        Preferences.Default.Remove(KeyEmail);
        Preferences.Default.Remove(KeyWhatsApp);
        Preferences.Default.Remove(KeyContatoNome);
        Preferences.Default.Remove(KeyContatoEmail);
        Preferences.Default.Remove(KeyContatoWhatsApp);
    }
}
