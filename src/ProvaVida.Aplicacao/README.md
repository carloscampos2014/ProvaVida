# ProvaVida.Aplicacao - Camada de Aplicação

## 📋 Visão Geral

A **Camada de Aplicação** é responsável pela **orquestração** entre o Domínio e a Infraestrutura. Aqui são implementados os **Use Cases** (ou Casos de Uso) através dos **Application Services**.

**Princípios Arquiteturais:**
- ✅ **Serviços Magros** - Apenas orquestração, regras pesadas no Domínio
- ✅ **DTOs com Barreira** - Entidades NUNCA saem para camadas superiores
- ✅ **Mapeamento Manual** - Controle total, sem AutoMapper
- ✅ **Segurança** - Campos sensíveis (SenhaHash, Telefone) nunca expostos
- ✅ **Injeção de Dependência** - Centralizada em ConfiguracaoAplicacao.cs

---

## 📁 Estrutura

```
ProvaVida.Aplicacao/
├── Dtos/                    # Data Transfer Objects (barreira de segurança)
│   ├── Usuarios/
│   │   ├── UsuarioRegistroDto.cs      # Entrada: criar usuário
│   │   ├── UsuarioResumoDto.cs        # Saída: dados públicos
│   │   └── UsuarioLoginDto.cs         # Entrada: autenticação
│   ├── CheckIns/
│   │   ├── CheckInRegistroDto.cs      # Entrada: registrar check-in
│   │   └── CheckInResumoDto.cs        # Saída: confirmação
│   ├── ContatosEmergencia/
│   │   ├── ContatoRegistroDto.cs      # Entrada: adicionar contato
│   │   └── ContatoResumoDto.cs        # Saída: dados do contato
│   └── Notificacoes/
│       ├── NotificacaoRegistroDto.cs  # Entrada: filtrar notificações
│       └── NotificacaoResumoDto.cs    # Saída: resumo de notificação
│
├── Mapeadores/              # Conversão manual: DTO ↔ Entidade
│   ├── UsuarioMapeador.cs
│   ├── CheckInMapeador.cs
│   ├── ContatoEmergenciaMapeador.cs
│   └── NotificacaoMapeador.cs
│
├── Servicos/                # Use Cases (Application Services)
│   ├── IAutenticacaoService.cs        # Interface
│   ├── AutenticacaoService.cs         # Implementação: registro + login
│   ├── ICheckInService.cs             # Interface
│   ├── CheckInService.cs              # Implementação: registrar check-in
│   ├── INotificacaoService.cs         # Interface (futura)
│   ├── NotificacaoService.cs          # Implementação (futura)
│   ├── IContatoEmergenciaService.cs   # Interface (futura)
│   └── ContatoEmergenciaService.cs    # Implementação (futura)
│
├── Exceções/                # Exceções de negócio da aplicação
│   └── AplicacaoException.cs          # Base + derivadas específicas
│
├── Configuracao/            # Setup de Injeção de Dependência
│   └── ConfiguracaoAplicacao.cs       # Extension: AdicionarAplicacao()
│
└── README.md                # Este arquivo
```

---

## 🔄 Padrão de Mapeamento Manual

### Exemplo: Usuário

```csharp
// DTO → Entidade (entrada)
var usuario = usuarioRegistroDto.ParaDominio(senhaHashBcrypt);

// Entidade → DTO (saída)
var resumoDto = usuario.ParaResumoDto();

// Lista → DTOs
var resumoDtos = usuarios.Select(u => u.ParaResumoDto()).ToList();
```

**Benefícios:**
- ✅ Controle total - cada campo é explícito
- ✅ Type-safe - o compilador garante tipos
- ✅ Auditável - fácil ver o que está sendo mapeado
- ✅ Seguro - SenhaHash nunca sai do Domínio

---

## 📦 DTOs (Data Transfer Objects)

### Padrão de Entrada (RegistroDto)

```csharp
public class UsuarioRegistroDto
{
    public string Nome { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Telefone { get; set; } = null!;
    public string Senha { get; set; } = null!;  // Texto plano
    
    public bool EhValido() => 
        !string.IsNullOrWhiteSpace(Nome) &&
        !string.IsNullOrWhiteSpace(Email) &&
        Senha.Length >= 6;
}
```

### Padrão de Saída (ResumoDto)

```csharp
public class UsuarioResumoDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Email { get; set; } = null!;
    public StatusUsuario Status { get; set; }
    
    // ❌ NUNCA incluir: SenhaHash, Telefone, histórico completo
}
```

---

## 🔐 Serviços de Aplicação

### AutenticacaoService

**Responsabilidades:**
1. Validar DTO de entrada
2. Verificar se email já existe
3. Criptografar senha com BCrypt
4. Invocar Factory do Domínio
5. Persistir via Repositório
6. Retornar DTO (sem dados sensíveis)

**Método:**
```csharp
Task<UsuarioResumoDto> RegistrarAsync(UsuarioRegistroDto dto, CancellationToken ct);
Task<UsuarioResumoDto> AutenticarAsync(UsuarioLoginDto dto, CancellationToken ct);
Task<bool> EmailJaExisteAsync(string email, CancellationToken ct);
```

### CheckInService

**Responsabilidades:**
1. Validar entrada
2. Buscar usuário
3. Verificar se está ativo
4. Invocar Usuario.RegistrarCheckIn() (Domínio)
5. Persistir atualização
6. Limpar notificações pendentes
7. Retornar confirmação

**Método:**
```csharp
Task<CheckInResumoDto> RegistrarCheckInAsync(CheckInRegistroDto dto, CancellationToken ct);
Task<List<CheckInResumoDto>> ObterHistoricoAsync(Guid usuarioId, CancellationToken ct);
```

---

## ⚙️ Configuração de Injeção de Dependência

No `Program.cs` da API:

```csharp
// Registrar camada de Aplicação
services.AdicionarAplicacao();

// Registrar Infraestrutura (BD, Repositórios)
services.AdicionarInfraestrutura(configuration);
```

### ConfiguracaoAplicacao.cs

```csharp
public static class ConfiguracaoAplicacao
{
    public static IServiceCollection AdicionarAplicacao(
        this IServiceCollection servicos)
    {
        servicos.AddScoped<IAutenticacaoService, AutenticacaoService>();
        servicos.AddScoped<ICheckInService, CheckInService>();
        // servicos.AddScoped<INotificacaoService, NotificacaoService>();
        // servicos.AddScoped<IContatoEmergenciaService, ContatoEmergenciaService>();
        
        return servicos;
    }
}
```

---

## 🛡️ Tratamento de Exceções

A camada de Aplicação define suas próprias exceções:

```csharp
public class AplicacaoException : Exception { }
public class UsuarioJaExisteException : AplicacaoException { }
public class UsuarioNaoEncontradoException : AplicacaoException { }
public class SenhaInvalidaException : AplicacaoException { }
public class UsuarioInativoException : AplicacaoException { }
public class ContatoNaoEncontradoException : AplicacaoException { }
```

**Fluxo:**
1. Domínio lança `UsuarioInvalidoException`
2. Service captura e relança como `AplicacaoException`
3. API captura `AplicacaoException` e mapeia para HTTP 400/409/401

---

## 🔄 Fluxo de Uma Requisição

### Exemplo: Registrar Usuário

```
CLIENTE HTTP
    ↓
    POST /api/usuarios/registrar
    {
        "nome": "João Silva",
        "email": "joao@example.com",
        "telefone": "11999998888",
        "senha": "MinhaS3nh@Forte"
    }
    ↓
API CONTROLLER
    ↓ 1. Recebe UsuarioRegistroDto
    ↓ 2. Valida DTO (estrutura)
    ↓ 3. Chama IAutenticacaoService.RegistrarAsync(dto)
    ↓
AUTENTICACAO SERVICE
    ↓ 4. Valida DTO (EhValido())
    ↓ 5. Verifica se email já existe
    ↓ 6. Criptografa senha (BCrypt)
    ↓ 7. Mapeia DTO → Usuario (ParaDominio)
    ↓
DOMINIO (FACTORY)
    ↓ 8. Usuario.Criar() valida regras pesadas
    ↓ 9. Lança UsuarioInvalidoException se inválido
    ↓ 10. Retorna Entidade se válido
    ↓
INFRAESTRUTURA (REPOSITÓRIO)
    ↓ 11. Persiste Usuario no banco
    ↓
SERVICE (continua)
    ↓ 12. Mapeia Usuario → UsuarioResumoDto
    ↓ 13. Retorna DTO para API
    ↓
API CONTROLLER
    ↓ 14. Retorna HTTP 201 + JSON
    ↓
CLIENTE
```

---

## 📊 Camadas de Uma Requisição

```
┌──────────────────────────────┐
│   HTTP / Controllers         │  ← API REST (Sprint 5)
└──────────────┬───────────────┘
               │
┌──────────────▼───────────────┐
│   APLICAÇÃO (Services)       │  ← ESTA CAMADA (Sprint 3)
│   - Orquestração             │
│   - Mapeamento (DTO)         │
│   - Validações estruturais   │
└──────────────┬───────────────┘
               │
┌──────────────▼───────────────┐
│   INFRAESTRUTURA             │  ← Repositórios, DbContext (Sprint 2)
│   - Persistência             │
│   - Acesso a Dados           │
│   - Serviços (BCrypt)        │
└──────────────┬───────────────┘
               │
┌──────────────▼───────────────┐
│   DOMÍNIO                    │  ← Regras de Negócio (Sprint 1)
│   - Entidades                │
│   - Factories                │
│   - Validações Pesadas       │
│   - Value Objects            │
└──────────────────────────────┘
```

---

## ✅ Checklist da Sprint 3 (Aplicação)

- [x] Projeto `ProvaVida.Aplicacao` criado
- [x] DTOs para Usuário (Registro, Resumo, Login)
- [x] DTOs para CheckIn (Registro, Resumo)
- [x] DTOs para ContatoEmergencia (Registro, Resumo)
- [x] DTOs para Notificação (Registro, Resumo)
- [x] Mapeadores manuais (4 arquivos)
- [x] AutenticacaoService (registro + login)
- [x] CheckInService (registrar + histórico)
- [x] Exceções de aplicação
- [x] ConfiguracaoAplicacao (DI)
- [x] Compilação: ✅ 0 erros

---

## 🚀 Próximas Pastas (Sprint 4+)

- [ ] NotificacaoService (consultar, limpar)
- [ ] ContatoEmergenciaService (CRUD)
- [ ] Testes unitários (xUnit) para Services
- [ ] Testes de integração com Repositórios mock
- [ ] API REST Controllers (Sprint 4)

---

## 📖 Referências

- [Clean Architecture - Uncle Bob](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- [Repository Pattern](https://martinfowler.com/eaaCatalog/repository.html)

---

**Última Atualização:** 31 de Janeiro de 2026  
**Sprint:** 3 - Camada de Aplicação  
**Status:** ✅ Completo
