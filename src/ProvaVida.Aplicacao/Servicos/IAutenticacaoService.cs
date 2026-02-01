using ProvaVida.Aplicacao.Dtos.Usuarios;

namespace ProvaVida.Aplicacao.Servicos;

/// <summary>
/// Interface para o serviço de autenticação.
/// Responsável por: registro, login e validação de credenciais.
/// </summary>
public interface IAutenticacaoService
{
    /// <summary>
    /// Registra um novo usuário no sistema.
    /// </summary>
    /// <param name="dto">Dados de registro (nome, email, telefone, senha).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>DTO com resumo do usuário criado.</returns>
    /// <exception cref="UsuarioJaExisteException">Se email já está registrado.</exception>
    /// <exception cref="Dominio.Exceções.UsuarioInvalidoException">Se dados inválidos.</exception>
    Task<UsuarioResumoDto> RegistrarAsync(
        UsuarioRegistroDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Autentica um usuário existente (valida credenciais).
    /// </summary>
    /// <param name="dto">Email e senha.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>DTO com resumo do usuário autenticado.</returns>
    /// <exception cref="UsuarioNaoEncontradoException">Se email não existe.</exception>
    /// <exception cref="SenhaInvalidaException">Se senha está errada.</exception>
    Task<UsuarioResumoDto> AutenticarAsync(
        UsuarioLoginDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um email já está registrado no sistema.
    /// </summary>
    /// <param name="email">Email para verificar.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>True se email existe, false caso contrário.</returns>
    Task<bool> EmailJaExisteAsync(string email, CancellationToken cancellationToken = default);
}
