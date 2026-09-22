using Microsoft.Extensions.Logging;
using ProvaVida.Api.Application.Commands;
using ProvaVida.Api.Application.Validators;
using ProvaVida.Api.Domain.Interfaces;
using ProvaVida.Shared.Common;
using ProvaVida.Shared.Dtos;
using ProvaVida.Shared.Entities;
using ProvaVida.Shared.Repositories;

namespace ProvaVida.Api.Application.Services;

/// <summary>
/// Serviço de aplicação para operações de autenticação: cadastro, login, refresh e logout.
/// </summary>
/// <remarks>
/// Orquestra validators FluentValidation, repositórios e o serviço de tokens JWT.
/// Nunca lança exceção para fluxos de negócio esperados — retorna <see cref="Result"/> ou
/// <see cref="Result{T}"/> em todos os casos.
/// </remarks>
public class AuthApplicationService : IAuthApplicationService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthApplicationService> _logger;

    /// <summary>
    /// Inicializa o serviço com suas dependências.
    /// </summary>
    public AuthApplicationService(
        IUsuarioRepository usuarioRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService,
        ILogger<AuthApplicationService> logger)
    {
        _usuarioRepository = usuarioRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<Usuario>> RegisterAsync(CadastrarUsuarioCommand command)
    {
        try
        {
            // Validação
            var validator = new CadastrarUsuarioValidator();
            var validation = await validator.ValidateAsync(command);
            if (!validation.IsValid)
            {
                var erros = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
                return Result<Usuario>.Fail(erros);
            }

            // Verifica e-mail duplicado
            var todos = await _usuarioRepository.GetAllAsync();
            if (todos.Success && todos.Data is not null)
            {
                var emailExiste = todos.Data.Any(u =>
                    u.Email.Equals(command.Email, StringComparison.OrdinalIgnoreCase));

                if (emailExiste)
                    return Result<Usuario>.Fail("E-mail já cadastrado.");
            }

            // Cria o usuário
            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = command.Nome,
                Email = command.Email.ToLowerInvariant(),
                Whatsapp = command.Whatsapp,
                SenhaHash = command.SenhaHash,
                ContatoEmergenciaNome = command.ContatoEmergenciaNome,
                ContatoEmergenciaEmail = command.ContatoEmergenciaEmail.ToLowerInvariant(),
                ContatoEmergenciaWhatsapp = command.ContatoEmergenciaWhatsapp,
                CriadoEm = DateTimeOffset.UtcNow,
                AtualizadoEm = DateTimeOffset.UtcNow
            };

            var resultado = await _usuarioRepository.UpsertAsync(usuario);
            if (!resultado.Success)
            {
                _logger.LogError("Falha ao persistir usuário {Email}: {Erro}", command.Email, resultado.MessageErro);
                return Result<Usuario>.Fail(resultado.MessageErro);
            }

            _logger.LogWarning("Usuário {UsuarioId} cadastrado com sucesso.", usuario.Id);
            return Result<Usuario>.Ok(usuario);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao cadastrar usuário {Email}.", command.Email);
            return Result<Usuario>.Fail("Erro interno ao processar o cadastro.");
        }
    }

    /// <inheritdoc />
    public async Task<Result<TokenResponse>> LoginAsync(LoginCommand command)
    {
        try
        {
            // Validação
            var validator = new LoginValidator();
            var validation = await validator.ValidateAsync(command);
            if (!validation.IsValid)
                return Result<TokenResponse>.Fail("Credenciais inválidas.");

            // Busca o usuário por e-mail e hash
            var todos = await _usuarioRepository.GetAllAsync();
            if (!todos.Success || todos.Data is null)
                return Result<TokenResponse>.Fail("Credenciais inválidas.");

            var usuario = todos.Data.FirstOrDefault(u =>
                u.Email.Equals(command.Email, StringComparison.OrdinalIgnoreCase) &&
                u.SenhaHash == command.SenhaHash);

            if (usuario is null)
            {
                _logger.LogWarning("Tentativa de login com credenciais inválidas para e-mail {Email}.", command.Email);
                return Result<TokenResponse>.Fail("Credenciais inválidas.");
            }

            // Gera tokens
            var accessToken = _tokenService.GenerateAccessToken(usuario);
            var refreshTokenValue = _tokenService.GenerateRefreshToken();

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuario.Id,
                Token = refreshTokenValue,
                ExpiraEm = DateTimeOffset.UtcNow.AddDays(7),
                Revogado = false,
                CriadoEm = DateTimeOffset.UtcNow
            };

            await _refreshTokenRepository.SaveAsync(refreshToken);

            _logger.LogWarning("Login bem-sucedido para usuário {UsuarioId}.", usuario.Id);

            return Result<TokenResponse>.Ok(new TokenResponse(
                AccessToken: accessToken,
                RefreshToken: refreshTokenValue,
                TokenType: "Bearer",
                ExpiresInSeconds: 3600
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao processar login para {Email}.", command.Email);
            return Result<TokenResponse>.Fail("Erro interno ao processar o login.");
        }
    }

    /// <inheritdoc />
    public async Task<Result<TokenResponse>> RefreshTokenAsync(RefreshTokenCommand command)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(command.RefreshToken))
                return Result<TokenResponse>.Fail("Refresh token inválido.");

            var tokenEntity = await _refreshTokenRepository.GetByTokenAsync(command.RefreshToken);

            if (tokenEntity is null)
                return Result<TokenResponse>.Fail("Refresh token não encontrado.");

            if (tokenEntity.Revogado)
                return Result<TokenResponse>.Fail("Refresh token revogado.");

            if (tokenEntity.ExpiraEm <= DateTimeOffset.UtcNow)
                return Result<TokenResponse>.Fail("Refresh token expirado.");

            // Busca o usuário
            var usuarioResult = await _usuarioRepository.GetByIdAsync(tokenEntity.UsuarioId);
            if (!usuarioResult.Success || usuarioResult.Data is null)
                return Result<TokenResponse>.Fail("Usuário não encontrado.");

            // Revoga o token antigo (Refresh Token Rotation)
            await _refreshTokenRepository.RevokeAsync(command.RefreshToken);

            // Gera novos tokens
            var novoAccessToken = _tokenService.GenerateAccessToken(usuarioResult.Data);
            var novoRefreshTokenValue = _tokenService.GenerateRefreshToken();

            var novoRefreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuarioResult.Data.Id,
                Token = novoRefreshTokenValue,
                ExpiraEm = DateTimeOffset.UtcNow.AddDays(7),
                Revogado = false,
                CriadoEm = DateTimeOffset.UtcNow
            };

            await _refreshTokenRepository.SaveAsync(novoRefreshToken);

            _logger.LogWarning("Refresh token rotacionado para usuário {UsuarioId}.", usuarioResult.Data.Id);

            return Result<TokenResponse>.Ok(new TokenResponse(
                AccessToken: novoAccessToken,
                RefreshToken: novoRefreshTokenValue,
                TokenType: "Bearer",
                ExpiresInSeconds: 3600
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao processar refresh token.");
            return Result<TokenResponse>.Fail("Erro interno ao renovar o token.");
        }
    }

    /// <inheritdoc />
    public async Task<Result> LogoutAsync(LogoutCommand command)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(command.RefreshToken))
                return Result.Fail("Refresh token inválido.");

            var tokenEntity = await _refreshTokenRepository.GetByTokenAsync(command.RefreshToken);
            if (tokenEntity is null)
                return Result.Fail("Refresh token não encontrado.");

            await _refreshTokenRepository.RevokeAsync(command.RefreshToken);

            _logger.LogWarning("Logout realizado, refresh token revogado para usuário {UsuarioId}.", tokenEntity.UsuarioId);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao processar logout.");
            return Result.Fail("Erro interno ao processar o logout.");
        }
    }
}
