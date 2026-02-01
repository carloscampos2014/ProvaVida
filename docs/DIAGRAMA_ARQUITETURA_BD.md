# 🎨 Diagrama: Arquitetura de Suporte a Múltiplos Bancos de Dados

## Fluxo de Configuração

```
┌────────────────────────────────────────────────────────────────────┐
│                       Program.cs / Startup                         │
│                                                                    │
│  var configDb = new ConfiguracaoBancoDados {                      │
│      Tipo = TipoProviderBancoDados.PostgreSQL,                    │
│      StringConexao = "Host=localhost;Database=provavida;..."      │
│  };                                                                │
│                                                                    │
│  services.AdicionarInfraestrutura(configDb);                      │
└────────────────────────────┬───────────────────────────────────────┘
                             │
                             ▼
┌────────────────────────────────────────────────────────────────────┐
│              ConfiguracaoInfraestrutura (Extensão)                │
│                                                                    │
│  public static IServiceCollection                                 │
│      AdicionarInfraestrutura(this IServiceCollection,             │
│                             ConfiguracaoBancoDados)               │
│                                                                    │
│  ✓ Registra DbContext                                             │
│  ✓ Registra Repositórios (IRepositorioUsuario, ...)              │
│  ✓ Registra Serviços (IServicoHashSenha)                         │
│  ✓ Chama Factory com ConfiguracaoBancoDados                      │
└────────────────────────────┬───────────────────────────────────────┘
                             │
                             ▼
┌────────────────────────────────────────────────────────────────────┐
│         ProviderBancoDadosFactory.ConfigurarProvedor()            │
│                                                                    │
│  Recebe:                                                           │
│   - DbContextOptionsBuilder                                       │
│   - ConfiguracaoBancoDados (Tipo + StringConexao)                 │
│                                                                    │
│  switch (configuracao.Tipo) {                                      │
│    case SQLite:     opcoes.UseSqlite(...)     break;              │
│    case PostgreSQL: opcoes.UseNpgsql(...)     break;              │
│    case SqlServer:  opcoes.UseSqlServer(...)  break;              │
│  }                                                                 │
└────────────────────────────┬───────────────────────────────────────┘
                             │
            ┌────────────────┼────────────────┐
            │                │                │
            ▼                ▼                ▼
    ┌──────────────┐  ┌──────────────┐  ┌──────────────┐
    │   SQLite     │  │ PostgreSQL   │  │  SQL Server  │
    │              │  │              │  │              │
    │ Development  │  │ Production   │  │ Production   │
    │ (Local)      │  │ (Linux/Mac)  │  │ (Windows)    │
    └──────────────┘  └──────────────┘  └──────────────┘
            │                │                │
            └────────────────┼────────────────┘
                             │
                             ▼
┌────────────────────────────────────────────────────────────────────┐
│         ProvaVidaDbContext (Agnóstico a BD)                       │
│                                                                    │
│  • DbSet<Usuario>                                                  │
│  • DbSet<CheckIn>                                                  │
│  • DbSet<ContatoEmergencia>                                        │
│  • DbSet<Notificacao>                                              │
│                                                                    │
│  ⚠️  NÃO sabe qual BD está usando!                               │
└────────────────────────────┬───────────────────────────────────────┘
                             │
                             ▼
┌────────────────────────────────────────────────────────────────────┐
│              Repositórios (Interfaces)                             │
│                                                                    │
│  • IRepositorioUsuario → RepositorioUsuario                       │
│  • IRepositorioCheckIn → RepositorioCheckIn                       │
│  • IRepositorioContatoEmergencia → Repositorio...                 │
│  • IRepositorioNotificacao → RepositorioNotificacao               │
│                                                                    │
│  ⚠️  NÃO sabem qual BD está usando!                              │
│  ✓ Funcionam igual com qualquer provedor                         │
└────────────────────────────┬───────────────────────────────────────┘
                             │
                             ▼
┌────────────────────────────────────────────────────────────────────┐
│        Camada de Aplicação (Use Cases / Services)                 │
│                                                                    │
│  • CriarUsuarioUseCase                                             │
│  • RealizarCheckInUseCase                                          │
│  • VerificaVencimentoUseCase                                       │
│  • EnviarNotificacaesUseCase                                       │
│                                                                    │
│  ✓ Lógica de negócio pura (independente de BD)                   │
└────────────────────────────────────────────────────────────────────┘
```

## Comparação: Antes vs. Depois

### ❌ ANTES (Arquitetura Rígida)

```csharp
// ConfiguracaoInfraestrutura.cs - HARDCODED SQLite
public static IServiceCollection AdicionarInfraestrutura(
    this IServiceCollection servicos,
    string stringConexao)
{
    servicos.AddDbContext<ProvaVidaDbContext>(opcoes =>
        opcoes.UseSqlite(stringConexao));  // ← FIXO! Não pode mudar
    
    // ... resto do código
}

// Resultado: Para mudar para PostgreSQL, você precisaria:
// 1. Modificar este arquivo
// 2. Recompilar
// 3. Fazer build de novo
```

### ✅ DEPOIS (Arquitetura Flexível)

```csharp
// ConfiguracaoInfraestrutura.cs - FLEXÍVEL
public static IServiceCollection AdicionarInfraestrutura(
    this IServiceCollection servicos,
    ConfiguracaoBancoDados configuracao)
{
    servicos.AddDbContext<ProvaVidaDbContext>(opcoes =>
        ProviderBancoDadosFactory.ConfigurarProvedor(opcoes, configuracao));
    // ... resto do código
}

// Program.cs - SÓ MUDAR CONFIG!
var configDb = new ConfiguracaoBancoDados
{
    Tipo = TipoProviderBancoDados.PostgreSQL,  // ← Trocar aqui
    StringConexao = "Host=localhost;Database=provavida;..."
};

servicos.AdicionarInfraestrutura(configDb);

// Resultado: Nenhum código precisa ser modificado!
```

## 🔑 Princípios SOLID Aplicados

| Princípio | Como Aplicado |
|-----------|----------------|
| **S**ingle Responsibility | Factory só cuida de configurar provedores |
| **O**pen/Closed | Aberto para novos provedores, fechado para mudanças |
| **L**iskov Substitution | Todos os provedores seguem mesmo contrato EF Core |
| **I**nterface Segregation | Interfaces específicas por tipo de repositório |
| **D**ependency Inversion | Injeção de dependência centralizada |

## 📊 Impacto na Arquitetura

```
Sem Factory Pattern:
  ProvaVida.Infraestrutura → ACOPLADO a SQLite
  Mudança = Modificar código + Recompilar + Deploy

Com Factory Pattern:
  ProvaVida.Infraestrutura → AGNÓSTICO a BD
  Mudança = Alterar configuração (ambiente, appsettings.json)
  
  ✓ Segue princípio Open/Closed
  ✓ Facilita testes (pode usar InMemory DB)
  ✓ Suporta estratégia por ambiente (Dev/Prod/Test)
```

## 🧪 Testabilidade

```csharp
// Teste com InMemoryDatabase (sem dependência de BD real)
[Fact]
public void TesteRepositorio()
{
    var configDb = new ConfiguracaoBancoDados
    {
        Tipo = TipoProviderBancoDados.InMemory,  // ← Fácil testar!
        StringConexao = "InMemory"
    };
    
    var options = new DbContextOptionsBuilder<ProvaVidaDbContext>();
    ProviderBancoDadosFactory.ConfigurarProvedor(options, configDb);
    
    // Teste rápido e sem I/O
}
```

---

**Conclusão**: A arquitetura atual permite adicionar novos provedores ou trocar de BD com **ZERO mudanças** no código de negócio. 🚀
