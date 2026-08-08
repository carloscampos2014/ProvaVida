using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProvaVida.Application.UseCases.CadastrarUsuario;
using ProvaVida.Application.UseCases.Login;
using ProvaVida.Application.UseCases.Logoff;

namespace ProvaVida.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    [HttpPost("cadastro")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cadastrar(
        [FromBody] CadastrarUsuarioInput input,
        [FromServices] CadastrarUsuarioUseCase useCase,
        CancellationToken ct)
    {
        var id = await useCase.ExecutarAsync(input, ct);
        return Created($"/conta/{id}", new { id });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginInput input,
        [FromServices] LoginUseCase useCase,
        CancellationToken ct)
    {
        var output = await useCase.ExecutarAsync(input, ct);
        return Ok(output);
    }

    [HttpPost("logoff")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logoff(
        [FromServices] LogoffUseCase useCase,
        CancellationToken ct)
    {
        var token = HttpContext.Request.Headers.Authorization
            .ToString()
            .Replace("Bearer ", string.Empty);

        await useCase.ExecutarAsync(new LogoffInput(token), ct);
        return NoContent();
    }
}
