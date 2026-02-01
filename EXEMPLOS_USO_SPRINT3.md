// EXEMPLO DE USO - SPRINT 3 CAMADA DE APLICAÇÃO

// ═══════════════════════════════════════════════════════════════════════════
// 1. CONFIGURAÇÃO NO Program.cs (Sprint 4 - API)
// ═══════════════════════════════════════════════════════════════════════════

/*
var builder = WebApplication.CreateBuilder(args);

// Registrar Infraestrutura (Repositórios, DbContext, BCrypt)
builder.Services.AdicionarInfraestrutura(builder.Configuration);

// Registrar Aplicação (Services, DTOs, Mapeadores)
builder.Services.AdicionarAplicacao();

var app = builder.Build();
app.Run();
*/

// ═══════════════════════════════════════════════════════════════════════════
// 2. USO EM UM CONTROLLER (Sprint 4 - API)
// ═══════════════════════════════════════════════════════════════════════════

/*
[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IAutenticacaoService _autenticacao;
    
    public UsuariosController(IAutenticacaoService autenticacao)
    {
        _autenticacao = autenticacao ?? throw new ArgumentNullException(nameof(autenticacao));
    }
    
    // POST /api/usuarios/registrar
    [HttpPost("registrar")]
    public async Task<ActionResult<UsuarioResumoDto>> RegistrarAsync(
        UsuarioRegistroDto dto,
        CancellationToken ct = default)
    {
        try
        {
            // A lógica é toda no Service
            var usuarioResumoDto = await _autenticacao.RegistrarAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = usuarioResumoDto.Id }, usuarioResumoDto);
        }
        catch (UsuarioJaExisteException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (AplicacaoException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    // POST /api/usuarios/login
    [HttpPost("login")]
    public async Task<ActionResult<UsuarioResumoDto>> AutenticarAsync(
        UsuarioLoginDto dto,
        CancellationToken ct = default)
    {
        try
        {
            var usuarioResumoDto = await _autenticacao.AutenticarAsync(dto, ct);
            // Aqui geraria JWT token (Sprint 4)
            return Ok(usuarioResumoDto);
        }
        catch (UsuarioNaoEncontradoException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (SenhaInvalidaException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }
}
*/

// ═══════════════════════════════════════════════════════════════════════════
// 3. FLUXO COMPLETO: REGISTRO DE USUÁRIO
// ═══════════════════════════════════════════════════════════════════════════

/*
CLIENT (Navegador)
  ↓
POST /api/usuarios/registrar
{
  "nome": "João Silva",
  "email": "joao@example.com",
  "telefone": "11999998888",
  "senha": "MinhaS3nh@Forte123"
}
  ↓
CONTROLLER
  ↓ 1. Recebe UsuarioRegistroDto (JSON → objeto)
  ↓ 2. Chama IAutenticacaoService.RegistrarAsync(dto, ct)
  ↓
AUTENTICACAO SERVICE
  ↓ 3. Valida DTO (EhValido())
  ↓ 4. Verifica se email já existe (EmailJaExisteAsync)
  ↓ 5. Se não existe:
  ↓     5.1. Criptografa senha com BCrypt (IServicoHashSenha.Hashar)
  ↓     5.2. Mapeia DTO → Entidade (ParaDominio)
  ↓
DOMÍNIO - FACTORY
  ↓ 6. Usuario.Criar() valida regras pesadas
  ↓     - Email válido (Value Object)
  ↓     - Telefone válido (Value Object)
  ↓     - Nome não vazio
  ↓ 7. Lança UsuarioInvalidoException se inválido
  ↓ 8. Retorna Entidade se válido
  ↓
SERVICE (continua)
  ↓ 9. Persiste Entidade (IRepositorioUsuario.AdicionarAsync)
  ↓
INFRAESTRUTURA
  ↓ 10. DbContext.SaveChangesAsync()
  ↓ 11. Retorna Entidade persistida
  ↓
SERVICE (continua)
  ↓ 12. Mapeia Entidade → DTO (ParaResumoDto)
  ↓     - Inclui: Id, Nome, Email, Status, DataProximoVencimento
  ↓     - Exclui: SenhaHash, Telefone
  ↓ 13. Retorna DTO
  ↓
CONTROLLER
  ↓ 14. Retorna HTTP 201 Created
  ↓
CLIENT
  ↓
HTTP 201 + JSON
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "nome": "João Silva",
  "email": "joao@example.com",
  "status": "Ativo",
  "dataProximoVencimento": "2026-02-02T12:00:00Z",
  "quantidadeContatos": 0,
  "dataCriacao": "2026-01-31T14:30:00Z"
}
*/

// ═══════════════════════════════════════════════════════════════════════════
// 4. FLUXO COMPLETO: REGISTRAR CHECK-IN
// ═══════════════════════════════════════════════════════════════════════════

/*
CLIENT (Navegador)
  ↓
POST /api/check-ins/registrar
{
  "usuarioId": "550e8400-e29b-41d4-a716-446655440000",
  "localizacao": "-23.550519,-46.633309"
}
  ↓
CONTROLLER
  ↓ 1. Recebe CheckInRegistroDto
  ↓ 2. Chama ICheckInService.RegistrarCheckInAsync(dto, ct)
  ↓
CHECK IN SERVICE
  ↓ 3. Valida DTO
  ↓ 4. Busca Usuario por ID (IRepositorioUsuario.ObterPorIdAsync)
  ↓ 5. Se não encontrado: lança UsuarioNaoEncontradoException
  ↓ 6. Se usuário está inativo: lança UsuarioInativoException
  ↓ 7. Mapeia DTO → CheckIn (ParaDominio)
  ↓
DOMÍNIO - FACTORY
  ↓ 8. CheckIn.Criar() valida regras
  ↓     - UsuarioId não vazio
  ↓     - DataCheckIn = DateTime.UtcNow (servidor decide)
  ↓     - DataProximoVencimento = DataCheckIn + 48h
  ↓ 9. Retorna Entidade
  ↓
SERVICE (continua)
  ↓ 10. Invoca Usuario.RegistrarCheckIn() (Domínio)
  ↓     - Atualiza DataProximoVencimento
  ↓     - Mantém histórico FIFO (máx 5)
  ↓ 11. Persiste atualização (IRepositorioUsuario.AtualizarAsync)
  ↓ 12. Limpa notificações pendentes
  ↓     - Busca notificações do usuário (IRepositorioNotificacao)
  ↓     - Cancela as que estão Pendentes
  ↓ 13. Mapeia CheckIn → CheckInResumoDto
  ↓ 14. Retorna DTO
  ↓
CONTROLLER
  ↓ 15. Retorna HTTP 200 OK
  ↓
CLIENT
  ↓
HTTP 200 + JSON
{
  "id": "660e8400-e29b-41d4-a716-446655440111",
  "usuarioId": "550e8400-e29b-41d4-a716-446655440000",
  "dataCheckIn": "2026-01-31T14:35:00Z",
  "dataProximoVencimento": "2026-02-02T14:35:00Z",
  "mensagem": "✅ Check-in realizado com sucesso! Próximo prazo em 48 horas."
}
*/

// ═══════════════════════════════════════════════════════════════════════════
// 5. EXEMPLO: MAPEAMENTO MANUAL
// ═══════════════════════════════════════════════════════════════════════════

/*
// DTO → Entidade
var usuarioRegistroDto = new UsuarioRegistroDto
{
    Nome = "Maria Silva",
    Email = "maria@example.com",
    Telefone = "11999997777",
    Senha = "S3nh@Fort123"
};

string senhaHash = servicoHash.Hashar(usuarioRegistroDto.Senha);
var usuarioEntidade = usuarioRegistroDto.ParaDominio(senhaHash);

// Agora usuarioEntidade é uma instância de Usuario (Domínio)
// com todas as validações já aplicadas

// Entidade → DTO
var usuarioResumoDto = usuarioEntidade.ParaResumoDto();

// SenhaHash nunca aparece em usuarioResumoDto!

// Lista → DTOs
var usuarios = await repositorio.ObterTodosAsync();
var resumoDtos = usuarios
    .Select(u => u.ParaResumoDto())
    .ToList();
*/

// ═══════════════════════════════════════════════════════════════════════════
// 6. EXEMPLO: TRATAMENTO DE ERROS
// ═══════════════════════════════════════════════════════════════════════════

/*
try
{
    var dto = new UsuarioRegistroDto
    {
        Nome = "",  // Inválido
        Email = "teste@example.com",
        Telefone = "11999998888",
        Senha = "123"  // Muito curta
    };
    
    await autenticacaoService.RegistrarAsync(dto);
}
catch (AplicacaoException ex)
{
    // "Dados de registro inválidos. Verifique nome, email, telefone e senha."
    logger.LogError(ex.Message);
    return BadRequest(ex.Message);
}
catch (UsuarioJaExisteException ex)
{
    // "Email 'teste@example.com' já registrado no sistema."
    logger.LogWarning(ex.Message);
    return Conflict(ex.Message);
}
catch (Exception ex)
{
    // Erro inesperado
    logger.LogError(ex, "Erro ao registrar usuário");
    return StatusCode(500, "Erro ao registrar usuário");
}
*/

// ═══════════════════════════════════════════════════════════════════════════
// 7. EXEMPLO: VALIDAÇÃO EM 2 CAMADAS
// ═══════════════════════════════════════════════════════════════════════════

/*
// CAMADA 1: DTO (Estrutura)
var dto = new UsuarioRegistroDto { /* ... */ };
if (!dto.EhValido())
    throw new AplicacaoException("Dados inválidos");

// CAMADA 2: Domínio (Negócio)
try
{
    var usuario = Usuario.Criar(
        nome: dto.Nome,
        email: dto.Email,
        telefone: dto.Telefone,
        senhaHash: senhaHash
    );
    // Factory valida:
    // - Email já existe?
    // - Formato de email válido?
    // - Formato de telefone válido?
    // - Nome entre 2-150 caracteres?
}
catch (UsuarioInvalidoException ex)
{
    // Regras de negócio rejeitaram
    throw new AplicacaoException($"Erro ao criar usuário: {ex.Message}");
}
*/

// ═══════════════════════════════════════════════════════════════════════════
// 8. EXEMPLO: INJEÇÃO DE DEPENDÊNCIA
// ═══════════════════════════════════════════════════════════════════════════

/*
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IRepositorioUsuario, RepositorioUsuario>();
builder.Services.AddScoped<IRepositorioCheckIn, RepositorioCheckIn>();
builder.Services.AddScoped<IServicoHashSenha, ServicoHashSenha>();

// Aqui ConfiguracaoAplicacao registra os Services
builder.Services.AdicionarAplicacao();
// Internamente:
//   services.AddScoped<IAutenticacaoService, AutenticacaoService>();
//   services.AddScoped<ICheckInService, CheckInService>();

var app = builder.Build();

// Em um Controller, o framework injeta automaticamente
[ApiController]
public class UsuariosController : ControllerBase
{
    public UsuariosController(IAutenticacaoService autenticacao)
    {
        // IAutenticacaoService foi criado com suas dependências:
        // - RepositorioUsuario
        // - ServicoHashSenha
    }
}
*/

// ═══════════════════════════════════════════════════════════════════════════
// 9. EXEMPLO: TESTES UNITÁRIOS (Sprint 5)
// ═══════════════════════════════════════════════════════════════════════════

/*
public class AutenticacaoServiceTests
{
    [Fact]
    public async Task RegistrarAsync_ComDtoValido_DeveCriarUsuario()
    {
        // Arrange
        var dto = new UsuarioRegistroDto
        {
            Nome = "João",
            Email = "joao@test.com",
            Telefone = "11999998888",
            Senha = "S3nh@Fort"
        };
        
        var mockRepositorio = new Mock<IRepositorioUsuario>();
        var mockHash = new Mock<IServicoHashSenha>();
        mockHash.Setup(x => x.Hashar(It.IsAny<string>()))
            .Returns("$2b$12$hash_bcrypt");
        
        var service = new AutenticacaoService(mockRepositorio.Object, mockHash.Object);
        
        // Act
        var resultado = await service.RegistrarAsync(dto);
        
        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("joao@test.com", resultado.Email);
        mockRepositorio.Verify(x => x.AdicionarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), 
            Times.Once);
    }
}
*/

// ═══════════════════════════════════════════════════════════════════════════
// FIM DOS EXEMPLOS
// ═══════════════════════════════════════════════════════════════════════════

// Para mais detalhes, consulte:
// - /src/ProvaVida.Aplicacao/README.md
// - /docs/ARQUITETURA.md
// - /docs/USER_STORIES.md
