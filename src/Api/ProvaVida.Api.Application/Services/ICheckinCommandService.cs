using ProvaVida.Api.Application.Commands;
using ProvaVida.Shared.Common;
using ProvaVida.Shared.Entities;

namespace ProvaVida.Api.Application.Services;

/// <summary>Contrato do serviço de comandos de check-in.</summary>
public interface ICheckinCommandService
{
    /// <summary>
    /// Cria um novo check-in diário para o usuário.
    /// Retorna <see cref="Result{T}.Fail"/> se já existe check-in do dia ou se os dados são inválidos.
    /// </summary>
    Task<Result<Checkin>> CreateAsync(CriarCheckinCommand command);
}
