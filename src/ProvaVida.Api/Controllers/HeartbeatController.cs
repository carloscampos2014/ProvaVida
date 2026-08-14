using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProvaVida.Application.UseCases.RegistrarHeartbeat;

namespace ProvaVida.Api.Controllers;

[ApiController]
[Route("heartbeat")]
[Authorize]
public class HeartbeatController : ControllerBase
{
    private Guid UsuarioId => Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException("Token inválido."));

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Registrar(
        [FromServices] RegistrarHeartbeatUseCase useCase,
        CancellationToken ct)
    {
        var input = new RegistrarHeartbeatInput(UsuarioId, DateTime.UtcNow);
        await useCase.ExecutarAsync(input, ct);
        return NoContent();
    }
}
