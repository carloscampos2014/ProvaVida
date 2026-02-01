using Microsoft.AspNetCore.Mvc;
using ProvaVida.Aplicacao.Dtos.CheckIns;
using ProvaVida.Aplicacao.Servicos;
using ProvaVida.API.Respostas;

namespace ProvaVida.API.Controllers;

/// <summary>
/// Controller para gerenciamento de Check-ins de Saúde.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class CheckInsController : ControllerBase
{
    private readonly ICheckInService _checkInService;
    private readonly ILogger<CheckInsController> _logger;

    public CheckInsController(ICheckInService checkInService, ILogger<CheckInsController> logger)
    {
        _checkInService = checkInService ?? throw new ArgumentNullException(nameof(checkInService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registra um novo check-in para o usuário.
    /// </summary>
    /// <param name="dto">Dados do check-in (usuarioId, localização opcional).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Check-in criado com ID.</returns>
    /// 
    /// <remarks>
    /// # Exemplo de Requisição
    /// 
    /// ```json
    /// POST /api/v1/check-ins/registrar
    /// Content-Type: application/json
    /// 
    /// {
    ///   "usuarioId": "550e8400-e29b-41d4-a716-446655440000",
    ///   "localizacao": "-23.550519,-46.633309"
    /// }
    /// ```
    /// </remarks>
    [HttpPost("registrar")]
    [ProducesResponseType(typeof(ApiResponse<CheckInResumoDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RegistrarAsync(
        [FromBody] CheckInRegistroDto dto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("✅ Check-in iniciado para usuário: {UsuarioId}", dto.UsuarioId);

        var checkInResumo = await _checkInService.RegistrarCheckInAsync(dto, cancellationToken);

        _logger.LogInformation("✅ Check-in registrado: {CheckInId}", checkInResumo.Id);

        return CreatedAtAction(
            actionName: nameof(ObterHistorico),
            routeValues: new { usuarioId = dto.UsuarioId },
            value: new ApiResponse<CheckInResumoDto>
            {
                Dados = checkInResumo,
                Mensagem = "✅ Check-in registrado com sucesso!",
                Timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Obtém o histórico de check-ins do usuário.
    /// </summary>
    /// <param name="usuarioId">ID do usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista de check-ins.</returns>
    /// 
    /// <remarks>
    /// # Exemplo de Requisição
    /// 
    /// ```
    /// GET /api/v1/check-ins/historico/550e8400-e29b-41d4-a716-446655440000
    /// ```
    /// </remarks>
    [HttpGet("historico/{usuarioId}")]
    [ProducesResponseType(typeof(ApiResponse<List<CheckInResumoDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterHistorico(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📋 Obtendo histórico de check-ins para: {UsuarioId}", usuarioId);

        var historico = await _checkInService.ObterHistoricoAsync(usuarioId, cancellationToken);

        return Ok(new ApiResponse<List<CheckInResumoDto>>
        {
            Dados = historico,
            Mensagem = $"✅ {historico.Count} check-ins encontrados.",
            Timestamp = DateTime.UtcNow
        });
    }
}
