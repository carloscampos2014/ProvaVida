namespace ProvaVida.Maui.Helpers;

/// <summary>
/// Validações simples e legíveis — sem regex complexo.
/// Foco em feedback claro para o usuário.
/// </summary>
public static class ValidacaoHelper
{
    /// <summary>
    /// Verifica se o e-mail tem formato básico válido (contém @ e ponto após @).
    /// </summary>
    public static bool EmailValido(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        var trimmed = email.Trim();
        var atIndex = trimmed.IndexOf('@');
        if (atIndex <= 0) return false;
        var dominio = trimmed[(atIndex + 1)..];
        return dominio.Contains('.') && dominio.Length >= 3;
    }

    /// <summary>
    /// Verifica se o WhatsApp tem pelo menos 10 dígitos (DDD + número).
    /// Aceita espaços, traços e parênteses — remove antes de contar.
    /// </summary>
    public static bool WhatsAppValido(string whatsApp)
    {
        if (string.IsNullOrWhiteSpace(whatsApp)) return false;
        var digitos = new string(whatsApp.Where(char.IsDigit).ToArray());
        return digitos.Length >= 10;
    }

    /// <summary>
    /// Mensagem de erro padrão para e-mail inválido.
    /// </summary>
    public const string MsgEmailInvalido = "Digite um e-mail válido (ex: nome@email.com).";

    /// <summary>
    /// Mensagem de erro padrão para WhatsApp inválido.
    /// </summary>
    public const string MsgWhatsAppInvalido = "Digite um WhatsApp válido com DDD (ex: 11999999999).";
}
