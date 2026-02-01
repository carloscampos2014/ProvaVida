using Microsoft.Extensions.Logging;
using ProvaVida.Aplicacao.Dtos.ContatosEmergencia;
using ProvaVida.Aplicacao.Exceções;
using ProvaVida.Aplicacao.Mapeadores;
using ProvaVida.Dominio.Repositorios;

namespace ProvaVida.Aplicacao.Servicos;

/// <summary>
/// Implementação do serviço de contatos de emergência.
/// </summary>
public class ContatoEmergenciaService : IContatoEmergenciaService
{
    private readonly IRepositorioContatoEmergencia _repositorio;
    private readonly IRepositorioUsuario _repositorioUsuario;
    private readonly ILogger<ContatoEmergenciaService> _logger;

    public ContatoEmergenciaService(
        IRepositorioContatoEmergencia repositorio,
        IRepositorioUsuario repositorioUsuario,
        ILogger<ContatoEmergenciaService> logger)
    {
        _repositorio = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        _repositorioUsuario = repositorioUsuario ?? throw new ArgumentNullException(nameof(repositorioUsuario));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<ContatoResumoDto> CriarAsync(Guid usuarioId, ContatoRegistroDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📝 Criando contato de emergência para usuário: {UsuarioId}", usuarioId);

        // Validar se usuário existe
        var usuario = await _repositorioUsuario.ObterPorIdAsync(usuarioId, cancellationToken);
        if (usuario == null)
        {
            throw new UsuarioNaoEncontradoException($"Usuário com ID {usuarioId} não encontrado.");
        }

        // Mapear DTO → Entidade (usando mapeador estático)
        var contato = dto.ParaDominio(usuarioId);

        // Persistir
        await _repositorio.AdicionarAsync(contato, cancellationToken);

        _logger.LogInformation("✅ Contato criado: {ContatoId}", contato.Id);

        // Retornar DTO
        return contato.ParaResumoDto();
    }

    /// <inheritdoc />
    public async Task<List<ContatoResumoDto>> ObterPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📋 Obtendo contatos do usuário: {UsuarioId}", usuarioId);

        // Validar se usuário existe
        var usuario = await _repositorioUsuario.ObterPorIdAsync(usuarioId, cancellationToken);
        if (usuario == null)
        {
            throw new UsuarioNaoEncontradoException($"Usuário com ID {usuarioId} não encontrado.");
        }

        // Buscar contatos
        var contatos = await _repositorio.ObterPorUsuarioIdAsync(usuarioId, cancellationToken);

        // Mapear para DTOs
        return contatos.Select(c => c.ParaResumoDto()).ToList();
    }

    /// <inheritdoc />
    public async Task<ContatoResumoDto> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🔍 Obtendo contato: {ContatoId}", id);

        var contato = await _repositorio.ObterPorIdAsync(id, cancellationToken);
        if (contato == null)
        {
            throw new ContatoNaoEncontradoException($"Contato com ID {id} não encontrado.");
        }

        return contato.ParaResumoDto();
    }

    /// <inheritdoc />
    public async Task<ContatoResumoDto> AtualizarAsync(Guid id, ContatoRegistroDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("✏️ Atualizando contato: {ContatoId}", id);

        var contato = await _repositorio.ObterPorIdAsync(id, cancellationToken);
        if (contato == null)
        {
            throw new ContatoNaoEncontradoException($"Contato com ID {id} não encontrado.");
        }

        // Para atualizar, remover e recriar com novos dados
        await _repositorio.RemoverAsync(id, cancellationToken);

        var contatoAtualizado = dto.ParaDominio(contato.UsuarioId);
        await _repositorio.AdicionarAsync(contatoAtualizado, cancellationToken);

        _logger.LogInformation("✅ Contato atualizado: {ContatoId}", id);

        return contatoAtualizado.ParaResumoDto();
    }

    /// <inheritdoc />
    public async Task RemoverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🗑️ Removendo contato: {ContatoId}", id);

        var contato = await _repositorio.ObterPorIdAsync(id, cancellationToken);
        if (contato == null)
        {
            throw new ContatoNaoEncontradoException($"Contato com ID {id} não encontrado.");
        }

        await _repositorio.RemoverAsync(id, cancellationToken);

        _logger.LogInformation("✅ Contato removido: {ContatoId}", id);
    }
}
