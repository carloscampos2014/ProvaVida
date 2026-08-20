using SQLite;

namespace ProvaVida.Maui.Models;

[Table("checkins_local")]
public class CheckInLocal
{
    [PrimaryKey]
    public string IdLocal { get; set; } = Guid.NewGuid().ToString();

    public string UsuarioId { get; set; } = string.Empty;
    public DateTimeOffset DataHora { get; set; } = DateTimeOffset.UtcNow;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public bool Sincronizado { get; set; } = false;
    public int TentativasSincronizacao { get; set; } = 0;
}
