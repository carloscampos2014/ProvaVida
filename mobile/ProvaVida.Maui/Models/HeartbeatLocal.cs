namespace ProvaVida.Maui.Models;

public class HeartbeatLocal
{
    public string IdLocal { get; set; } = Guid.NewGuid().ToString();
    public string UsuarioId { get; set; } = string.Empty;
    /// <summary>
    /// Data/hora em UTC — armazenada como string "yyyy-MM-ddTHH:mm:ssZ".
    /// </summary>
    public string DataHora { get; set; } = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
    public bool Sincronizado { get; set; } = false;
}
