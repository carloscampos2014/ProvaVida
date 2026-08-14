using ProvaVida.Application.Interfaces;
using ProvaVida.Domain.Entities;

namespace ProvaVida.Application.UseCases.RegistrarHeartbeat;

public class RegistrarHeartbeatUseCase
{
    private readonly IHeartbeatRepository _heartbeatRepository;
    private readonly IUnitOfWork _uow;

    public RegistrarHeartbeatUseCase(IHeartbeatRepository heartbeatRepository, IUnitOfWork uow)
    {
        _heartbeatRepository = heartbeatRepository;
        _uow = uow;
    }

    public async Task ExecutarAsync(RegistrarHeartbeatInput input, CancellationToken ct = default)
    {
        var heartbeat = Heartbeat.Criar(input.UsuarioId, input.DataHora);

        await _uow.BeginAsync(cancellationToken: ct);
        try
        {
            await _heartbeatRepository.AdicionarAsync(heartbeat, ct);
            await _uow.CommitAsync(ct);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}
