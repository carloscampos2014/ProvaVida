using Bogus;
using FluentAssertions;
using FluentValidation.TestHelper;
using ProvaVida.Api.Application.Commands;
using ProvaVida.Api.Application.Validators;

namespace ProvaVida.Api.Tests.UseCases;

/// <summary>
/// Testes unitários para <see cref="CadastrarUsuarioValidator"/> e <see cref="LoginValidator"/>.
/// </summary>
public class ValidatorsTests
{
    private readonly CadastrarUsuarioValidator _cadastroValidator = new();
    private readonly LoginValidator _loginValidator = new();
    private readonly Faker _faker = new("pt_BR");

    private CadastrarUsuarioCommand ComandoCadastroValido() =>
        new(
            Nome: _faker.Name.FullName(),
            Email: _faker.Internet.Email(),
            Whatsapp: _faker.Random.ReplaceNumbers("##########"),
            SenhaHash: _faker.Random.AlphaNumeric(64),
            ContatoEmergenciaNome: _faker.Name.FullName(),
            ContatoEmergenciaEmail: _faker.Internet.Email(),
            ContatoEmergenciaWhatsapp: _faker.Random.ReplaceNumbers("##########")
        );

    // ─── CadastrarUsuarioValidator ─────────────────────────────────────────

    [Fact]
    public async Task CadastrarUsuarioValidator_ComDadosValidos_DevePassar()
    {
        var result = await _cadastroValidator.TestValidateAsync(ComandoCadastroValido());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task CadastrarUsuarioValidator_ComNomeVazio_DeveFalhar()
    {
        var command = ComandoCadastroValido() with { Nome = "" };
        var result = await _cadastroValidator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Nome);
    }

    [Fact]
    public async Task CadastrarUsuarioValidator_ComEmailInvalido_DeveFalhar()
    {
        var command = ComandoCadastroValido() with { Email = "nao-e-email" };
        var result = await _cadastroValidator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task CadastrarUsuarioValidator_ComWhatsappInvalido_DeveFalhar()
    {
        var command = ComandoCadastroValido() with { Whatsapp = "123" }; // menos de 10 dígitos
        var result = await _cadastroValidator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Whatsapp);
    }

    [Fact]
    public async Task CadastrarUsuarioValidator_ComSenhaHashCurta_DeveFalhar()
    {
        var command = ComandoCadastroValido() with { SenhaHash = "hash-curto" };
        var result = await _cadastroValidator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.SenhaHash);
    }

    [Fact]
    public async Task CadastrarUsuarioValidator_ComEmailContatoInvalido_DeveFalhar()
    {
        var command = ComandoCadastroValido() with { ContatoEmergenciaEmail = "invalido" };
        var result = await _cadastroValidator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.ContatoEmergenciaEmail);
    }

    // ─── LoginValidator ────────────────────────────────────────────────────

    [Fact]
    public async Task LoginValidator_ComDadosValidos_DevePassar()
    {
        var command = new LoginCommand(
            Email: _faker.Internet.Email(),
            SenhaHash: _faker.Random.AlphaNumeric(64));
        var result = await _loginValidator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task LoginValidator_ComEmailVazio_DeveFalhar()
    {
        var command = new LoginCommand(Email: "", SenhaHash: _faker.Random.AlphaNumeric(64));
        var result = await _loginValidator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task LoginValidator_ComEmailInvalido_DeveFalhar()
    {
        var command = new LoginCommand(Email: "nao-e-email", SenhaHash: _faker.Random.AlphaNumeric(64));
        var result = await _loginValidator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task LoginValidator_ComSenhaHashInvalida_DeveFalhar()
    {
        var command = new LoginCommand(Email: _faker.Internet.Email(), SenhaHash: "curto");
        var result = await _loginValidator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.SenhaHash);
    }
}
