using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ProvaVida.Api.Infrastructure.Services;
using ProvaVida.Shared.Entities;

namespace ProvaVida.Api.Tests.Services;

/// <summary>
/// Testes unitários para <see cref="JwtTokenService"/>.
/// </summary>
public class JwtTokenServiceTests
{
    private readonly JwtTokenService _service;
    private readonly Usuario _usuario;

    public JwtTokenServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "test-secret-key-com-pelo-menos-32-chars-para-hmac256",
                ["Jwt:ExpiresInMinutes"] = "60"
            })
            .Build();

        var logger = new Mock<ILogger<JwtTokenService>>();
        _service = new JwtTokenService(config, logger.Object);

        var faker = new Faker<Usuario>("pt_BR")
            .RuleFor(u => u.Id, f => f.Random.Guid())
            .RuleFor(u => u.Nome, f => f.Name.FullName())
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.Whatsapp, f => f.Phone.PhoneNumber("###########"))
            .RuleFor(u => u.SenhaHash, f => f.Random.Hash())
            .RuleFor(u => u.ContatoEmergenciaNome, f => f.Name.FullName())
            .RuleFor(u => u.ContatoEmergenciaEmail, f => f.Internet.Email())
            .RuleFor(u => u.ContatoEmergenciaWhatsapp, f => f.Phone.PhoneNumber("###########"))
            .RuleFor(u => u.CriadoEm, f => f.Date.PastOffset())
            .RuleFor(u => u.AtualizadoEm, f => f.Date.RecentOffset());

        _usuario = faker.Generate();
    }

    // ── GenerateAccessToken ────────────────────────────────────────────────

    [Fact]
    public void GenerateAccessToken_DeveRetornarJwtValido()
    {
        // Act
        var token = _service.GenerateAccessToken(_usuario);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();

        var partes = token.Split('.');
        partes.Should().HaveCount(3, "JWT deve ter header.payload.signature");
    }

    [Fact]
    public void GenerateAccessToken_DeveConterClaimsCorretas()
    {
        // Act
        var token = _service.GenerateAccessToken(_usuario);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c =>
            c.Type == ClaimTypes.NameIdentifier && c.Value == _usuario.Id.ToString(),
            "deve conter claim NameIdentifier com o Id do usuário");

        jwt.Claims.Should().Contain(c =>
            c.Type == ClaimTypes.Email && c.Value == _usuario.Email,
            "deve conter claim Email com o email do usuário");

        jwt.Claims.Should().Contain(c =>
            c.Type == ClaimTypes.Name && c.Value == _usuario.Nome,
            "deve conter claim Name com o nome do usuário");
    }

    // ── GenerateRefreshToken ───────────────────────────────────────────────

    [Fact]
    public void GenerateRefreshToken_DeveRetornarStringBase64Unica()
    {
        // Act
        var token1 = _service.GenerateRefreshToken();
        var token2 = _service.GenerateRefreshToken();

        // Assert
        token1.Should().NotBeNullOrWhiteSpace();
        token2.Should().NotBeNullOrWhiteSpace();
        token1.Should().NotBe(token2, "dois refresh tokens não devem ser iguais");

        // Verifica que é Base64 válido
        var act = () => Convert.FromBase64String(token1);
        act.Should().NotThrow("o token deve ser uma string Base64 válida");
    }

    [Fact]
    public void GenerateRefreshToken_DeveTer64BytesDeEntropy()
    {
        // Act
        var token = _service.GenerateRefreshToken();

        // Assert
        var bytes = Convert.FromBase64String(token);
        bytes.Should().HaveCount(64, "o refresh token deve ter exatamente 64 bytes de entropia");
    }

    // ── GetPrincipalFromExpiredToken ───────────────────────────────────────

    [Fact]
    public void GetPrincipalFromExpiredToken_ComTokenValido_DeveRetornarClaims()
    {
        // Arrange — gera um token com configuração de expiração de 0 minutos para simular token expirado
        var configExpirado = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "test-secret-key-com-pelo-menos-32-chars-para-hmac256",
                ["Jwt:ExpiresInMinutes"] = "-1" // já expirado ao ser criado
            })
            .Build();

        var logger = new Mock<ILogger<JwtTokenService>>();
        var serviceExpirado = new JwtTokenService(configExpirado, logger.Object);
        var tokenExpirado = serviceExpirado.GenerateAccessToken(_usuario);

        // Act
        var principal = _service.GetPrincipalFromExpiredToken(tokenExpirado);

        // Assert
        principal.Should().NotBeNull("token com assinatura válida deve retornar claims mesmo expirado");
        principal!.FindFirst(ClaimTypes.NameIdentifier)?.Value
            .Should().Be(_usuario.Id.ToString());
        principal.FindFirst(ClaimTypes.Email)?.Value
            .Should().Be(_usuario.Email);
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_ComTokenInvalido_DeveRetornarNull()
    {
        // Arrange — token com assinatura diferente (outra chave)
        var configOutraChave = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "outra-chave-secreta-totalmente-diferente-32-chars",
                ["Jwt:ExpiresInMinutes"] = "60"
            })
            .Build();

        var logger = new Mock<ILogger<JwtTokenService>>();
        var serviceOutroKey = new JwtTokenService(configOutraChave, logger.Object);
        var tokenAssinadoComOutraChave = serviceOutroKey.GenerateAccessToken(_usuario);

        // Act — tenta validar com a chave original
        var principal = _service.GetPrincipalFromExpiredToken(tokenAssinadoComOutraChave);

        // Assert
        principal.Should().BeNull("token assinado com chave diferente deve retornar null");
    }
}
