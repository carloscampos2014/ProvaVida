using Microsoft.Extensions.Logging;
using ProvaVida.Api.Application.Commands;
using ProvaVida.Api.Application.Validators;
using ProvaVida.Shared.Common;
using ProvaVida.Shared.Entities;
using ProvaVida.Shared.Repositories;

namespace ProvaVida.Api.Application.Services;

/// <summary>
/// Serviço de aplicação para comandos de check-in diário.
/// </summary>
/// <remarks>
/// Orquestra validação, verificação de duplicata e persistência via <see cref="ICheckinRepository"/>.
/// Nunca lança exceção para fluxos de negócio esperados — retorna <see cref="Result{T}"/> em todos os casos.
/// </remarks>
public class CheckinCommandService : ICheckinCommandService
{
    private readonly ICheckinRepository _repository;
    private readonly ILogger<CheckinCommandService> _logger;

    /// <summary>Inicializa o serviço com suas dependências.</summary>
    public CheckinCommandService(
        ICheckinRepository repository,
        ILogger<CheckinCommandService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<Checkin>> CreateAsync(CriarCheckinCommand command)
    {
        try
        {
            // Validação de entrada
            var validator = new CriarCheckinValidator();
            var validation = await validator.ValidateAsync(command);
            if (!validation.IsValid)
            {
                var erros = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("Check-in inválido para usuário {UsuarioId}: {Erros}", command.UsuarioId, erros);
                return Result<Checkin>.Fail(erros);
            }

            // Verificar duplicata: 1 check-in por dia por usuário
            var existente = await _repository.GetByUsuarioIdAndDataAsync(command.UsuarioId, command.Data);
            if (existente.Success)
            {
                _logger.LogWarning("Check-in duplicado para usuário {UsuarioId} na data {Data}.", command.UsuarioId, command.Data);
                return Result<Checkin>.Fail("Já existe um check-in para hoje");
            }

            // Criar entidade
            var checkin = new Checkin
            {
                Id = Guid.NewGuid(),
                UsuarioId = command.UsuarioId,
                Data = command.Data,
                Latitude = command.Latitude,
                Longitude = command.Longitude,
                IdentificacaoAparelho = command.IdentificacaoAparelho,
                Sincronizado = true,
                CriadoEm = DateTimeOffset.UtcNow
            };

            // Persistir
            var resultado = await _repository.UpsertAsync(checkin);
            if (!resultado.Success)
            {
                _logger.LogError("Falha ao persistir check-in para usuário {UsuarioId}: {Erro}", command.UsuarioId, resultado.MessageErro);
                return Result<Checkin>.Fail(resultado.MessageErro);
            }

            _logger.LogInformation("Check-in {CheckinId} criado para usuário {UsuarioId} em {Data}.", checkin.Id, checkin.UsuarioId, checkin.Data);
            return Result<Checkin>.Ok(checkin);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao processar check-in para usuário {UsuarioId}.", command.UsuarioId);
            return Result<Checkin>.Fail("Erro interno ao processar o check-in.");
        }
    }
}
