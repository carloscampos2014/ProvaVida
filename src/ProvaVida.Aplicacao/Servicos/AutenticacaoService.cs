using ProvaVida.Aplicacao.Dtos.Usuarios;
using ProvaVida.Aplicacao.Exceções;
using ProvaVida.Aplicacao.Mapeadores;
using ProvaVida.Dominio.Exceções;
using ProvaVida.Dominio.Repositorios;
using ProvaVida.Infraestrutura.Servicos;
using Microsoft.Extensions.Logging;

namespace ProvaVida.Aplicacao.Servicos;

/// <summary>
/// Implementação do serviço de autenticação.
/// 
/// Responsabilidades:
/// - Validar entrada (DTOs)
/// - Orquestrar Domínio + Infraestrutura
/// - Mapear resultados para DTOs
/// - Registrar contato de emergência junto com o usuário
/// 
/// REGRAS PESADAS ficam no Domínio (Usuario.Criar, validações de Email, etc).
/// Este serviço apenas ORQUESTRA as chamadas.
/// </summary>
public class AutenticacaoService : IAutenticacaoService
{
    private readonly IRepositorioUsuario _repositorioUsuario;
    private readonly IRepositorioContatoEmergencia _repositorioContato;
    private readonly IServicoHashSenha _servicoHash;
    private readonly ILogger<AutenticacaoService> _logger;

    /// <summary>
    /// Construtor com injeção de dependência.
    /// </summary>
    /// <param name="repositorioUsuario">Repositório para persistência de usuários.</param>
    /// <param name="repositorioContato">Repositório para persistência de contatos.</param>
    /// <param name="servicoHash">Serviço de criptografia (BCrypt).</param>
    /// <param name="logger">Logger para diagnosticar fluxos.</param>
    /// <exception cref="ArgumentNullException">Se alguma dependência é nula.</exception>
    public AutenticacaoService(
        IRepositorioUsuario repositorioUsuario,
        IRepositorioContatoEmergencia repositorioContato,
        IServicoHashSenha servicoHash,
        ILogger<AutenticacaoService> logger)
    {
        _repositorioUsuario = repositorioUsuario ?? 
            throw new ArgumentNullException(nameof(repositorioUsuario));
        _repositorioContato = repositorioContato ?? 
            throw new ArgumentNullException(nameof(repositorioContato));
        _servicoHash = servicoHash ?? 
            throw new ArgumentNullException(nameof(servicoHash));
        _logger = logger ?? 
            throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registra um novo usuário no sistema COM contato de emergência obrigatório.
    /// 
    /// FLUXO:
    /// 1️⃣ Validar DTO (estrutura, tipos)
    /// 2️⃣ Verificar se email já existe
    /// 3️⃣ Verificar se email do contato já existe
    /// 4️⃣ Criptografar senha com BCrypt
    /// 5️⃣ Chamar Factory do Domínio (Usuario.Criar) - faz validações pesadas
    /// 6️⃣ Persistir usuário no banco via repositório
    /// 7️⃣ Criar e persistir contato de emergência
    /// 8️⃣ Mapear para DTO e retornar
    /// </summary>
    public async Task<UsuarioResumoDto> RegistrarAsync(
        UsuarioRegistroDto dto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📝 Iniciando registro de novo usuário: {Email}", dto.Email);

        // 1️⃣ Validar entrada (estrutura, tipos)
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "DTO de registro não pode ser nulo.");

        if (!dto.EhValido())
            throw new AplicacaoException("Dados de registro inválidos. Verifique nome, email, telefone e senha.");

        if (dto.ContatoEmergencia == null)
            throw new AplicacaoException("Contato de emergência é obrigatório para registrar uma conta.");

        // 2️⃣ Verificar se email do usuário já existe
        var emailJaExiste = await EmailJaExisteAsync(dto.Email, cancellationToken);
        if (emailJaExiste)
        {
            _logger.LogWarning("⚠️ Tentativa de registro com email duplicado: {Email}", dto.Email);
            throw new UsuarioJaExisteException($"Email '{dto.Email}' já registrado no sistema.");
        }

        // 3️⃣ Verificar se email do contato já existe (como email de contato de outro usuário)
        var emailContatoJaExiste = await _repositorioContato.ObterPorEmailAsync(
            dto.ContatoEmergencia.Email, 
            cancellationToken);
        if (emailContatoJaExiste != null)
        {
            _logger.LogWarning("⚠️ Email de contato duplicado: {Email}", dto.ContatoEmergencia.Email);
            throw new AplicacaoException(
                $"Email de contato '{dto.ContatoEmergencia.Email}' já está registrado como contato de outro usuário.");
        }

        // 4️⃣ Criptografar senha com BCrypt (12 rounds padrão)
        var senhaHash = _servicoHash.Hashar(dto.Senha);

        // 5️⃣ Chamar Factory do Domínio (faz validações pesadas de negócio)
        try
        {
            var usuario = dto.ParaDominio(senhaHash);

            // 6️⃣ Persistir usuário no banco
            await _repositorioUsuario.AdicionarAsync(usuario, cancellationToken);

            _logger.LogInformation("✅ Usuário criado: {UsuarioId}", usuario.Id);

            // 7️⃣ Criar e persistir contato de emergência
            var contato = dto.ContatoEmergencia.ParaDominio(usuario.Id);
            await _repositorioContato.AdicionarAsync(contato, cancellationToken);

            _logger.LogInformation("✅ Contato de emergência criado: {ContatoId}", contato.Id);

            // 8️⃣ Retornar DTO (sem expor dados sensíveis)
            return usuario.ParaResumoDto();
        }
        catch (UsuarioInvalidoException ex)
        {
            // O Domínio rejeitou - re-lançar como exceção de aplicação
            throw new AplicacaoException($"Erro ao criar usuário: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Autentica um usuário existente (login).
    /// 
    /// FLUXO:
    /// 1️⃣ Buscar usuário por email no banco
    /// 2️⃣ Validar senha com BCrypt (Verify)
    /// 3️⃣ Retornar resumo do usuário
    /// </summary>
    public async Task<UsuarioResumoDto> AutenticarAsync(
        UsuarioLoginDto dto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🔐 Tentativa de login: {Email}", dto.Email);

        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "DTO de login não pode ser nulo.");

        // 1️⃣ Buscar usuário
        var usuario = await _repositorioUsuario.ObterPorEmailAsync(
            dto.Email,
            cancellationToken);

        if (usuario == null)
        {
            _logger.LogWarning("⚠️ Usuário não encontrado: {Email}", dto.Email);
            throw new UsuarioNaoEncontradoException(
                $"Usuário com email '{dto.Email}' não encontrado.");
        }

        // 2️⃣ Validar senha (BCrypt.Verify)
        bool senhaValida = _servicoHash.Verificar(dto.Senha, usuario.SenhaHash);

        if (!senhaValida)
        {
            _logger.LogWarning("⚠️ Senha inválida para: {Email}", dto.Email);
            throw new SenhaInvalidaException("Senha incorreta.");
        }

        _logger.LogInformation("✅ Login bem-sucedido: {UsuarioId}", usuario.Id);

        // 3️⃣ Retornar resumo (sem expor SenhaHash)
        return usuario.ParaResumoDto();
    }

    /// <summary>
    /// Verifica se um email já está registrado no sistema.
    /// </summary>
    public async Task<bool> EmailJaExisteAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var usuario = await _repositorioUsuario.ObterPorEmailAsync(email, cancellationToken);
        return usuario != null;
    }

    /// <summary>
    /// Obtém o perfil/resumo de um usuário pelo ID.
    /// </summary>
    public async Task<UsuarioResumoDto> ObterPerfilPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🔎 Buscando perfil do usuário: {UsuarioId}", id);
        var usuario = await _repositorioUsuario.ObterPorIdAsync(id, cancellationToken);
        if (usuario == null)
        {
            _logger.LogWarning("⚠️ Usuário não encontrado: {UsuarioId}", id);
            throw new UsuarioNaoEncontradoException($"Usuário com id '{id}' não encontrado.");
        }
        return usuario.ParaResumoDto();
    }
}
