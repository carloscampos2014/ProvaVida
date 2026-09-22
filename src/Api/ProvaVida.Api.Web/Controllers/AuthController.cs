using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProvaVida.Api.Application.Commands;
using ProvaVida.Api.Application.Services;
using ProvaVida.Shared.Dtos;

namespace ProvaVida.Api.Web.Controllers;

/// <summary>
/// Controller de autenticação — cadastro, login, renovação e invalidação de tokens.
/// </summary>
[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthApplicationService _authService;
    private readonly ILogger<AuthController> _logger;

    /// <summary>
    /// Inicializa o controller com o serviço de autenticação e o logger.
    /// </summary>
    /// <param name="authService">Serviço de aplicação de autenticação.</param>
    /// <param name="logger">Logger para eventos do controller.</param>
    public AuthController(IAuthApplicationService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Cadastra um novo usuário no sistema.
    /// </summary>
    /// <param name="request">Dados do novo usuário.</param>
    /// <returns>201 Created com o usuário criado, 400/409 em caso de falha.</returns>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var command = CadastrarUsuarioCommand.FromRequest(request);
        var result = await _authService.RegisterAsync(command);

        if (!result.Success)
        {
            _logger.LogWarning("Falha no cadastro para {Email}: {Erro}", request.Email, result.MessageErro);

            if (result.MessageErro?.Contains("já cadastrado", StringComparison.OrdinalIgnoreCase) == true)
                return Conflict(new { error = result.MessageErro });

            return BadRequest(new { error = result.MessageErro });
        }

        var usuario = result.Data!;
        return CreatedAtAction(nameof(Register), new
        {
            id = usuario.Id,
            nome = usuario.Nome,
            email = usuario.Email,
            whatsapp = usuario.Whatsapp,
            contatoEmergenciaNome = usuario.ContatoEmergenciaNome,
            contatoEmergenciaEmail = usuario.ContatoEmergenciaEmail,
            contatoEmergenciaWhatsapp = usuario.ContatoEmergenciaWhatsapp,
            criadoEm = usuario.CriadoEm
        });
    }

    /// <summary>
    /// Autentica um usuário e retorna os tokens de acesso.
    /// </summary>
    /// <param name="request">Credenciais do usuário (e-mail e hash SHA-256).</param>
    /// <returns>200 OK com tokens, 401 Unauthorized se credenciais inválidas.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = LoginCommand.FromRequest(request);
        var result = await _authService.LoginAsync(command);

        if (!result.Success)
        {
            _logger.LogWarning("Falha no login para {Email}.", request.Email);
            return Unauthorized(new { error = "Credenciais inválidas." });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Renova os tokens usando o refresh token atual (Refresh Token Rotation).
    /// </summary>
    /// <param name="request">Refresh token atual.</param>
    /// <returns>200 OK com novos tokens, 401 Unauthorized se inválido/expirado.</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);
        var result = await _authService.RefreshTokenAsync(command);

        if (!result.Success)
        {
            _logger.LogWarning("Falha ao renovar refresh token.");
            return Unauthorized(new { error = result.MessageErro });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Encerra a sessão do usuário revogando o refresh token informado.
    /// </summary>
    /// <param name="request">Refresh token a ser revogado.</param>
    /// <returns>204 No Content se bem-sucedido, 400 Bad Request se não encontrado.</returns>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        var command = new LogoutCommand(request.RefreshToken);
        var result = await _authService.LogoutAsync(command);

        if (!result.Success)
        {
            _logger.LogWarning("Falha no logout: {Erro}", result.MessageErro);
            return BadRequest(new { error = result.MessageErro });
        }

        return NoContent();
    }
}
