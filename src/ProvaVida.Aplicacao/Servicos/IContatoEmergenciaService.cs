using ProvaVida.Aplicacao.Dtos.ContatosEmergencia;

namespace ProvaVida.Aplicacao.Servicos;

/// <summary>
/// Serviço de aplicação para gerenciamento de contatos de emergência.
/// 
/// Responsabilidades:
/// - Orquestrar validações de negócio
/// - Coordenar repositórios
/// - Mapear DTOs → Entidades
/// - Retornar DTOs de resposta
/// 
/// Não expõe entidades de domínio diretamente.
/// </summary>
public interface IContatoEmergenciaService
{
    /// <summary>
    /// Cria um novo contato de emergência para um usuário.
    /// </summary>
    Task<ContatoResumoDto> CriarAsync(Guid usuarioId, ContatoRegistroDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todos os contatos de um usuário.
    /// </summary>
    Task<List<ContatoResumoDto>> ObterPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém um contato específico por ID.
    /// </summary>
    Task<ContatoResumoDto> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza um contato existente.
    /// </summary>
    Task<ContatoResumoDto> AtualizarAsync(Guid id, ContatoRegistroDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove um contato.
    /// </summary>
    Task RemoverAsync(Guid id, CancellationToken cancellationToken = default);
}
