namespace ProvaVida.Maui.Models;

public class CheckInLocal
{
    public string IdLocal { get; set; } = Guid.NewGuid().ToString();
    public string UsuarioId { get; set; } = string.Empty;
    /// <summary>
    /// Data/hora em UTC — armazenada como string "yyyy-MM-ddTHH:mm:ssZ".
    /// String nativo do SQLite elimina problemas de mapeamento DateTimeOffset via Dapper.
    /// </summary>
    public string DataHora { get; set; } = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public bool Sincronizado { get; set; } = false;
    public int TentativasSincronizacao { get; set; } = 0;
}
