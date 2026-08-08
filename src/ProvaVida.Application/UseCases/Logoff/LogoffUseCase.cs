using ProvaVida.Application.Common;
using ProvaVida.Application.Interfaces;

namespace ProvaVida.Application.UseCases.Logoff;

public class LogoffUseCase
{
    private readonly ISessaoLoginRepository _sessaoRepository;
    private readonly IUnitOfWork _uow;

    public LogoffUseCase(ISessaoLoginRepository sessaoRepository, IUnitOfWork uow)
    {
        _sessaoRepository = sessaoRepository;
        _uow = uow;
    }

    public async Task ExecutarAsync(LogoffInput input, CancellationToken ct = default)
    {
        var sessao = await _sessaoRepository.ObterPorTokenAsync(input.Token, ct);

        if (sessao is null || !sessao.EstaValida())
            throw AppException.NaoAutorizado("Sessão inválida ou já encerrada.");

        sessao.Invalidar();

        await _uow.BeginAsync(cancellationToken: ct);
        try
        {
            await _sessaoRepository.SalvarAlteracoesAsync(ct);
            await _uow.CommitAsync(ct);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}
