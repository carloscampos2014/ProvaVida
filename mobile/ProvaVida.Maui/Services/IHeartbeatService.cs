namespace ProvaVida.Maui.Services;

public interface IHeartbeatService
{
    Task EnviarAsync(CancellationToken ct = default);
}
