using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProvaVida.Application.UseCases.ObterHistoricoCheckIn;
using ProvaVida.Application.UseCases.RegistrarCheckIn;

namespace ProvaVida.Api.Controllers;

[ApiController]
[Route("checkin")]
[Authorize]
public class CheckInController : ControllerBase
{
    private Guid UsuarioId => Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException("Token inválido."));

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Registrar(
        [FromBody] RegistrarCheckInRequest request,
        [FromServices] RegistrarCheckInUseCase useCase,
        CancellationToken ct)
    {
        var input = new RegistrarCheckInInput(
            UsuarioId,
            request.IdLocal,
            request.DataHora,
            request.Latitude,
            request.Longitude,
            request.DeviceId ?? string.Empty);

        var inserido = await useCase.ExecutarAsync(input, ct);

        // 204 = inserido, 200 = já existia (idempotente)
        return inserido ? NoContent() : Ok(new { mensagem = "Check-in já registrado anteriormente." });
    }

    [HttpGet("historico")]
    [ProducesResponseType(typeof(IEnumerable<CheckInDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Historico(
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim,
        [FromServices] ObterHistoricoCheckInUseCase useCase,
        CancellationToken ct)
    {
        var input = new ObterHistoricoCheckInInput(UsuarioId, dataInicio, dataFim);
        var resultado = await useCase.ExecutarAsync(input, ct);
        return Ok(resultado);
    }
}

public record RegistrarCheckInRequest(
    Guid IdLocal,
    DateTime DataHora,
    double? Latitude,
    double? Longitude,
    string? DeviceId
);
