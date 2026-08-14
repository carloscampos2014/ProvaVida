using SQLite;

namespace ProvaVida.Maui.Models;

[Table("heartbeats_local")]
public class HeartbeatLocal
{
    [PrimaryKey]
    public string IdLocal { get; set; } = Guid.NewGuid().ToString();

    public string UsuarioId { get; set; } = string.Empty;
    public DateTime DataHora { get; set; } = DateTime.UtcNow;
    public bool Sincronizado { get; set; } = false;
}
