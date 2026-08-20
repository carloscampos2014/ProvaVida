using SQLite;

namespace ProvaVida.Maui.Models;

[Table("heartbeats_local")]
public class HeartbeatLocal
{
    [PrimaryKey]
    public string IdLocal { get; set; } = Guid.NewGuid().ToString();

    public string UsuarioId { get; set; } = string.Empty;
    public DateTimeOffset DataHora { get; set; } = DateTimeOffset.UtcNow;
    public bool Sincronizado { get; set; } = false;
}
