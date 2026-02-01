using Microsoft.AspNetCore.Mvc;
using ProvaVida.Aplicacao.Dtos.Usuarios;
using ProvaVida.Aplicacao.Servicos;
using ProvaVida.API.Respostas;

namespace ProvaVida.API.Controllers;

/// <summary>
/// Controller para autenticação (Registro e Login).
/// 
/// Responsabilidades:
/// - Receber HTTP requests
/// - Validar com FluentValidation (automático)
/// - Chamar IAutenticacaoService
/// - Retornar ActionResult com status code correto
/// 
/// LIMPO: Sem lógica de negócio (tudo no Service).
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAutenticacaoService _autenticacao;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAutenticacaoService autenticacao, ILogger<AuthController> logger)
    {
        _autenticacao = autenticacao ?? throw new ArgumentNullException(nameof(autenticacao));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registra um novo usuário no sistema.
    /// </summary>
    /// <param name="dto">Dados do novo usuário (nome, email, telefone, senha).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Usuário criado com ID.</returns>
    /// 
    /// <remarks>
    /// # Exemplo de Requisição
    /// 
    /// ```json
    /// POST /api/v1/auth/registrar
    /// Content-Type: application/json
    /// 
    /// {
    ///   "nome": "João Silva",
    ///   "email": "joao@example.com",
    ///   "telefone": "11987654321",
    ///   "senha": "MinhaS3nh@Forte"
    /// }
    /// ```
    /// </remarks>
    [HttpPost("registrar")]
    [ProducesResponseType(typeof(ApiResponse<UsuarioResumoDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RegistrarAsync(
        [FromBody] UsuarioRegistroDto dto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📝 Tentativa de registro: {Email}", dto.Email);

        try
        {
            // 🔍 Validação: FluentValidation faz automaticamente (antes de chegar aqui)
            // 🔗 Chamar Service
            var usuarioResumo = await _autenticacao.RegistrarAsync(dto, cancellationToken);

            // ✅ Retornar 201 Created
            _logger.LogInformation("✅ Usuário registrado: {UsuarioId}", usuarioResumo.Id);

            return CreatedAtAction(
                actionName: nameof(GetPerfil),
                routeValues: new { id = usuarioResumo.Id },
                value: new ApiResponse<UsuarioResumoDto>
                {
                    Dados = usuarioResumo,
                    Mensagem = "✅ Usuário registrado com sucesso!",
                    Timestamp = DateTime.UtcNow
                });
        }
        catch (Exception ex)
        {
            // ❌ Exceções são capturadas por GlobalExceptionMiddleware
            // Controller não trata, middleware retorna status code correto
            _logger.LogError(ex, "❌ Erro ao registrar usuário: {Email}", dto.Email);
            throw;  // Re-lança para middleware capturar
        }
    }

    /// <summary>
    /// Autentica um usuário existente (Login).
    /// </summary>
    /// <param name="dto">Email e senha.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Dados do usuário autenticado.</returns>
    /// 
    /// <remarks>
    /// # Exemplo de Requisição
    /// 
    /// ```json
    /// POST /api/v1/auth/login
    /// Content-Type: application/json
    /// 
    /// {
    ///   "email": "joao@example.com",
    ///   "senha": "MinhaS3nh@Forte"
    /// }
    /// ```
    /// 
    /// # Resposta de Sucesso (200 OK)
    /// ```json
    /// {
    ///   "dados": {
    ///     "id": "550e8400-e29b-41d4-a716-446655440000",
    ///     "nome": "João Silva",
    ///     "email": "joao@example.com",
    ///     "status": "Ativo"
    ///   },
    ///   "mensagem": "✅ Autenticado com sucesso!"
    /// }
    /// ```
    /// </remarks>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<UsuarioResumoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AutenticarAsync(
        [FromBody] UsuarioLoginDto dto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🔐 Tentativa de login: {Email}", dto.Email);

        var usuarioResumo = await _autenticacao.AutenticarAsync(dto, cancellationToken);

        _logger.LogInformation("✅ Login bem-sucedido: {UsuarioId}", usuarioResumo.Id);

        return Ok(new ApiResponse<UsuarioResumoDto>
        {
            Dados = usuarioResumo,
            Mensagem = "✅ Autenticado com sucesso!",
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Obtém o perfil do usuário autenticado (futura - com JWT).
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UsuarioResumoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult GetPerfil(Guid id)
    {
        // Futura implementação (Sprint 4+) - com autenticação JWT
        return Ok(new { mensagem = "Implementação futura" });
    }
}
