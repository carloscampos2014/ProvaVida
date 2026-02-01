using ProvaVida.Aplicacao.Dtos.Usuarios;
using ProvaVida.Aplicacao.Exceções;
using ProvaVida.Aplicacao.Mapeadores;
using ProvaVida.Dominio.Exceções;
using ProvaVida.Dominio.Repositorios;
using ProvaVida.Infraestrutura.Servicos;

namespace ProvaVida.Aplicacao.Servicos;

/// <summary>
/// Implementação do serviço de autenticação.
/// 
/// Responsabilidades:
/// - Validar entrada (DTOs)
/// - Orquestrar Domínio + Infraestrutura
/// - Mapear resultados para DTOs
/// 
/// REGRAS PESADAS ficam no Domínio (Usuario.Criar, validações de Email, etc).
/// Este serviço apenas ORQUESTRA as chamadas.
/// </summary>
public class AutenticacaoService : IAutenticacaoService
{
    private readonly IRepositorioUsuario _repositorioUsuario;
    private readonly IServicoHashSenha _servicoHash;

    /// <summary>
    /// Construtor com injeção de dependência.
    /// </summary>
    /// <param name="repositorioUsuario">Repositório para persistência de usuários.</param>
    /// <param name="servicoHash">Serviço de criptografia (BCrypt).</param>
    /// <exception cref="ArgumentNullException">Se alguma dependência é nula.</exception>
    public AutenticacaoService(
        IRepositorioUsuario repositorioUsuario,
        IServicoHashSenha servicoHash)
    {
        _repositorioUsuario = repositorioUsuario ?? 
            throw new ArgumentNullException(nameof(repositorioUsuario));
        _servicoHash = servicoHash ?? 
            throw new ArgumentNullException(nameof(servicoHash));
    }

    /// <summary>
    /// Registra um novo usuário no sistema.
    /// 
    /// FLUXO:
    /// 1️⃣ Validar DTO (estrutura, tipos)
    /// 2️⃣ Verificar se email já existe
    /// 3️⃣ Criptografar senha com BCrypt
    /// 4️⃣ Chamar Factory do Domínio (Usuario.Criar) - faz validações pesadas
    /// 5️⃣ Persistir no banco via repositório
    /// 6️⃣ Mapear para DTO e retornar
    /// </summary>
    public async Task<UsuarioResumoDto> RegistrarAsync(
        UsuarioRegistroDto dto,
        CancellationToken cancellationToken = default)
    {
        // 1️⃣ Validar entrada (estrutura, tipos)
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "DTO de registro não pode ser nulo.");

        if (!dto.EhValido())
            throw new AplicacaoException("Dados de registro inválidos. Verifique nome, email, telefone e senha.");

        // 2️⃣ Verificar se email já existe
        var emailJaExiste = await EmailJaExisteAsync(dto.Email, cancellationToken);
        if (emailJaExiste)
            throw new UsuarioJaExisteException($"Email '{dto.Email}' já registrado no sistema.");

        // 3️⃣ Criptografar senha com BCrypt (12 rounds padrão)
        var senhaHash = _servicoHash.Hashar(dto.Senha);

        // 4️⃣ Chamar Factory do Domínio (faz validações pesadas de negócio)
        try
        {
            var usuario = dto.ParaDominio(senhaHash);

            // 5️⃣ Persistir no banco
            await _repositorioUsuario.AdicionarAsync(usuario, cancellationToken);

            // 6️⃣ Retornar DTO (sem expor dados sensíveis)
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
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "DTO de login não pode ser nulo.");

        // 1️⃣ Buscar usuário
        var usuario = await _repositorioUsuario.ObterPorEmailAsync(
            dto.Email,
            cancellationToken);

        if (usuario == null)
            throw new UsuarioNaoEncontradoException(
                $"Usuário com email '{dto.Email}' não encontrado.");

        // 2️⃣ Validar senha (BCrypt.Verify)
        bool senhaValida = _servicoHash.Verificar(dto.Senha, usuario.SenhaHash);

        if (!senhaValida)
            throw new SenhaInvalidaException("Senha incorreta.");

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
}
