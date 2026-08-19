namespace ProvaVida.Domain.Entities;

public class NotificacaoEmergencia
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public string Canal { get; private set; } = string.Empty;
    public DateTime DataDisparo { get; private set; }
    public DateTime? JanelaExpiraEm { get; private set; }

    public static class Statuses
    {
        public const string HeartbeatAtivo      = "heartbeat_ativo";
        public const string AguardandoResposta  = "aguardando_resposta";
        public const string Cancelado           = "cancelado";
        public const string Disparado           = "disparado";
    }

    protected NotificacaoEmergencia() { }

    public static NotificacaoEmergencia CriarHeartbeatAtivo(Guid usuarioId)
        => new()
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Status = Statuses.HeartbeatAtivo,
            Canal = "nenhum",
            DataDisparo = DateTime.UtcNow
        };

    public static NotificacaoEmergencia CriarAguardandoResposta(Guid usuarioId, int janelaHoras = 6)
        => new()
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Status = Statuses.AguardandoResposta,
            Canal = "email_usuario",
            DataDisparo = DateTime.UtcNow,
            JanelaExpiraEm = DateTime.UtcNow.AddHours(janelaHoras)
        };

    public static NotificacaoEmergencia CriarDisparado(Guid usuarioId, string canal)
        => new()
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Status = Statuses.Disparado,
            Canal = canal,
            DataDisparo = DateTime.UtcNow
        };

    public void Cancelar()
    {
        Status = Statuses.Cancelado;
    }

    /// <summary>
    /// Marca o registro aguardando_resposta como processado após o disparo ao contato.
    /// Evita que o job horário reprocesse o mesmo registro indefinidamente.
    /// </summary>
    public void MarcarComoProcessado(string canal)
    {
        Status = Statuses.Disparado;
        Canal  = canal;
    }
}
