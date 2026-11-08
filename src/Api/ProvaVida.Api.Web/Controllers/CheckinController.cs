using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProvaVida.Api.Application.Commands;
using ProvaVida.Api.Application.Services;
using ProvaVida.Shared.Dtos;

namespace ProvaVida.Api.Web.Controllers;

/// <summary>
/// Controller de check-in diário — registra a prova de vida do usuário.
/// </summary>
[ApiController]
[Route("checkins")]
[Authorize]
public class CheckinController : ControllerBase
{
    private readonly ICheckinCommandService _checkinService;
    private readonly ILogger<CheckinController> _logger;

    /// <summary>
    /// Inicializa o controller com o serviço de check-in e o logger.
    /// </summary>
    /// <param name="checkinService">Serviço de aplicação para comandos de check-in.</param>
    /// <param name="logger">Logger para eventos do controller.</param>
    public CheckinController(ICheckinCommandService checkinService, ILogger<CheckinController> logger)
    {
        _checkinService = checkinService;
        _logger = logger;
    }

    /// <summary>
    /// Registra um novo check-in diário para o usuário autenticado.
    /// </summary>
    /// <param name="request">Dados do check-in: data, coordenadas e identificação do aparelho.</param>
    /// <returns>
    /// 201 Created com o check-in criado;
    /// 409 Conflict se já existe check-in para o dia;
    /// 400 Bad Request para dados inválidos;
    /// 401 Unauthorized se não autenticado.
    /// </returns>
    [HttpPost]
    [ProducesResponseType(typeof(CheckinResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CheckinRequest request)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (usuarioIdClaim is null || !Guid.TryParse(usuarioIdClaim, out var usuarioId))
        {
            _logger.LogWarning("Check-in rejeitado: claim NameIdentifier ausente ou inválida.");
            return Unauthorized(new { error = "Token inválido." });
        }

        var command = new CriarCheckinCommand(
            UsuarioId: usuarioId,
            Data: request.Data,
            Latitude: request.Latitude,
            Longitude: request.Longitude,
            IdentificacaoAparelho: request.IdentificacaoAparelho
        );

        var result = await _checkinService.CreateAsync(command);

        if (!result.Success)
        {
            _logger.LogWarning("Falha no check-in para usuário {UsuarioId}: {Erro}", usuarioId, result.MessageErro);

            if (result.MessageErro?.Contains("Já existe", StringComparison.OrdinalIgnoreCase) == true)
                return Conflict(new { error = result.MessageErro });

            return BadRequest(new { error = result.MessageErro });
        }

        var response = result.Data!.ToDto();
        _logger.LogInformation("Check-in {CheckinId} criado para usuário {UsuarioId}.", response.Id, usuarioId);
        return CreatedAtAction(nameof(Create), new { id = response.Id }, response);
    }
}
