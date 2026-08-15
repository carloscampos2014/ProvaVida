using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProvaVida.Application.Interfaces;
using ProvaVida.Application.UseCases.AlterarConta;
using ProvaVida.Application.UseCases.AlterarSenha;
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

    [HttpGet]
    [ProducesResponseType(typeof(ContaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obter(
        [FromServices] IUsuarioRepository usuarioRepository,
        CancellationToken ct)
    {
        var usuario = await usuarioRepository.ObterPorIdAsync(UsuarioId, ct);
        if (usuario is null) return NotFound();

        return Ok(new ContaResponse(
            usuario.Nome,
            usuario.Email,
            usuario.WhatsApp,
            usuario.ContatoEmergenciaNome,
            usuario.ContatoEmergenciaEmail,
            usuario.ContatoEmergenciaWhatsApp));
    }

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

    [HttpPut("senha")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AlterarSenha(
        [FromBody] AlterarSenhaRequest request,
        [FromServices] AlterarSenhaUseCase useCase,
        CancellationToken ct)
    {
        var input = new AlterarSenhaInput(UsuarioId, request.SenhaAtual, request.NovaSenha);
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

public record AlterarSenhaRequest(string SenhaAtual, string NovaSenha);

public record ContaResponse(
    string Nome,
    string Email,
    string WhatsApp,
    string ContatoEmergenciaNome,
    string ContatoEmergenciaEmail,
    string ContatoEmergenciaWhatsApp);

public record AlterarContaRequest(
    string Nome,
    string WhatsApp,
    string ContatoEmergenciaNome,
    string ContatoEmergenciaEmail,
    string ContatoEmergenciaWhatsApp);

public record ExcluirContaRequest(string Senha);
