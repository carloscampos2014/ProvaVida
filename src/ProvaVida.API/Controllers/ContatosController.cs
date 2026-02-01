using Microsoft.AspNetCore.Mvc;
using ProvaVida.Aplicacao.Dtos.ContatosEmergencia;
using ProvaVida.Aplicacao.Servicos;
using ProvaVida.API.Respostas;

namespace ProvaVida.API.Controllers;

/// <summary>
/// Controller para gerenciamento de Contatos de Emergência.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class ContatosController : ControllerBase
{
    private readonly IContatoEmergenciaService _contatoService;
    private readonly ILogger<ContatosController> _logger;

    public ContatosController(IContatoEmergenciaService contatoService, ILogger<ContatosController> logger)
    {
        _contatoService = contatoService ?? throw new ArgumentNullException(nameof(contatoService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Cria um novo contato de emergência para um usuário.
    /// </summary>
    /// <param name="usuarioId">ID do usuário.</param>
    /// <param name="dto">Dados do contato (nome, email, whatsapp, prioridade).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Contato criado.</returns>
    /// 
    /// <remarks>
    /// # Exemplo de Requisição
    /// 
    /// ```json
    /// POST /api/v1/contatos/550e8400-e29b-41d4-a716-446655440000
    /// Content-Type: application/json
    /// 
    /// {
    ///   "nome": "Maria Silva",
    ///   "email": "maria@example.com",
    ///   "whatsApp": "11987654321",
    ///   "prioridade": 1
    /// }
    /// ```
    /// </remarks>
    [HttpPost("{usuarioId}")]
    [ProducesResponseType(typeof(ApiResponse<ContatoResumoDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CriarAsync(
        Guid usuarioId,
        [FromBody] ContatoRegistroDto dto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("👤 Criando contato de emergência para usuário: {UsuarioId}", usuarioId);

        var contatoResumo = await _contatoService.CriarAsync(usuarioId, dto, cancellationToken);

        _logger.LogInformation("✅ Contato criado: {ContatoId}", contatoResumo.Id);

        return CreatedAtAction(
            actionName: nameof(ObterPorIdAsync),
            routeValues: new { usuarioId, id = contatoResumo.Id },
            value: new ApiResponse<ContatoResumoDto>
            {
                Dados = contatoResumo,
                Mensagem = "✅ Contato criado com sucesso!",
                Timestamp = DateTime.UtcNow
            });
    }

    /// <summary>
    /// Obtém todos os contatos de emergência de um usuário.
    /// </summary>
    /// <param name="usuarioId">ID do usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista de contatos.</returns>
    /// 
    /// <remarks>
    /// # Exemplo de Requisição
    /// 
    /// ```
    /// GET /api/v1/contatos/550e8400-e29b-41d4-a716-446655440000
    /// ```
    /// </remarks>
    [HttpGet("{usuarioId}")]
    [ProducesResponseType(typeof(ApiResponse<List<ContatoResumoDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📋 Obtendo contatos do usuário: {UsuarioId}", usuarioId);

        var contatos = await _contatoService.ObterPorUsuarioAsync(usuarioId, cancellationToken);

        return Ok(new ApiResponse<List<ContatoResumoDto>>
        {
            Dados = contatos,
            Mensagem = $"✅ {contatos.Count} contatos encontrados.",
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Obtém um contato específico por ID.
    /// </summary>
    [HttpGet("{usuarioId}/{id}")]
    [ProducesResponseType(typeof(ApiResponse<ContatoResumoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorIdAsync(
        Guid usuarioId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🔍 Obtendo contato: {ContatoId}", id);

        var contato = await _contatoService.ObterPorIdAsync(id, cancellationToken);

        return Ok(new ApiResponse<ContatoResumoDto>
        {
            Dados = contato,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Atualiza um contato de emergência.
    /// </summary>
    /// <param name="usuarioId">ID do usuário.</param>
    /// <param name="id">ID do contato.</param>
    /// <param name="dto">Dados atualizados.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Contato atualizado.</returns>
    [HttpPut("{usuarioId}/{id}")]
    [ProducesResponseType(typeof(ApiResponse<ContatoResumoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarAsync(
        Guid usuarioId,
        Guid id,
        [FromBody] ContatoRegistroDto dto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("✏️ Atualizando contato: {ContatoId}", id);

        var contatoAtualizado = await _contatoService.AtualizarAsync(id, dto, cancellationToken);

        return Ok(new ApiResponse<ContatoResumoDto>
        {
            Dados = contatoAtualizado,
            Mensagem = "✅ Contato atualizado com sucesso!",
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Remove um contato de emergência.
    /// </summary>
    /// <param name="usuarioId">ID do usuário.</param>
    /// <param name="id">ID do contato.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Sem conteúdo (204 No Content).</returns>
    [HttpDelete("{usuarioId}/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoverAsync(
        Guid usuarioId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🗑️ Removendo contato: {ContatoId}", id);

        await _contatoService.RemoverAsync(id, cancellationToken);

        _logger.LogInformation("✅ Contato removido");

        return NoContent();
    }
}
