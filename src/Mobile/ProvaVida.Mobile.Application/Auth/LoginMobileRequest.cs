namespace ProvaVida.Mobile.Application.Auth;

/// <summary>
/// Dados de entrada do formulário de login antes da geração do hash SHA-256.
/// Utilizado exclusivamente pelo <see cref="Validators.LoginMobileValidator"/>.
/// </summary>
/// <param name="Email">E-mail informado pelo usuário.</param>
/// <param name="Senha">Senha em texto puro — descartada após validação e hash.</param>
public record LoginMobileRequest(string Email, string Senha);
