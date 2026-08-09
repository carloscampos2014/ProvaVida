using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProvaVida.Application.UseCases.AlterarConta;
using ProvaVida.Application.UseCases.ExcluirConta;

namespace ProvaVida.Api.Controllers;

[ApiController]
[Route("conta")]
[Authorize]
public class ContaController : ControllerBase
{
    private Guid UsuarioId => Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException("Token inválido."));

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Alterar(
        [FromBody] AlterarContaRequest request,
        [FromServices] AlterarContaUseCase useCase,
        CancellationToken ct)
    {
        var input = new AlterarContaInput(
            UsuarioId,
            request.Nome,
            request.WhatsApp,
            request.ContatoEmergenciaNome,
            request.ContatoEmergenciaEmail,
            request.ContatoEmergenciaWhatsApp);

        await useCase.ExecutarAsync(input, ct);
        return NoContent();
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(
        [FromBody] ExcluirContaRequest request,
        [FromServices] ExcluirContaUseCase useCase,
        CancellationToken ct)
    {
        var input = new ExcluirContaInput(UsuarioId, request.Senha);
        await useCase.ExecutarAsync(input, ct);
        return NoContent();
    }
}

public record AlterarContaRequest(
    string Nome,
    string WhatsApp,
    string ContatoEmergenciaNome,
    string ContatoEmergenciaEmail,
    string ContatoEmergenciaWhatsApp);

public record ExcluirContaRequest(string Senha);
