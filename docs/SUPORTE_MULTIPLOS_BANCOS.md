# 🔄 Suporte a Múltiplos Bancos de Dados

## Arquitetura Flexível de Provedores

A infraestrutura do **ProvaVida** foi desenhada para permitir **trocar de banco de dados sem modificar o código existente** usando o padrão **Factory Pattern**.

## 🏗️ Como Funciona

### Estrutura de Camadas

```
┌─────────────────────────────────────────┐
│  Aplicação / API                        │
├─────────────────────────────────────────┤
│  Repositórios (IRepositorioUsuario...)  │ ← Agnósticos a BD
├─────────────────────────────────────────┤
│  DbContext (ProvaVidaDbContext)         │ ← Configurável
├─────────────────────────────────────────┤
│  ProviderBancoDadosFactory              │ ← Factory Pattern
├─────────────────────────────────────────┤
│  SQLite / PostgreSQL / SQL Server       │ ← Providers intercambiáveis
└─────────────────────────────────────────┘
```

**Chave**: Os **repositórios usam interfaces** que não sabem qual BD está sendo usado. A configuração é centralizada na **Factory**.

## 📝 Exemplos de Uso

### 1️⃣ Usando SQLite (Padrão Atual)

```csharp
// Program.cs ou Startup.cs
var builder = WebApplicationBuilder.CreateBuilder(args);

var configDb = new ConfiguracaoBancoDados
{
    Tipo = TipoProviderBancoDados.SQLite,
    StringConexao = "Data Source=provavida.db"
};

builder.Services.AdicionarInfraestrutura(configDb);

var app = builder.Build();
```

### 2️⃣ Trocar para PostgreSQL (Sem Mexer em Nada Mais!)

```csharp
// Program.cs - Única mudança necessária
var configDb = new ConfiguracaoBancoDados
{
    Tipo = TipoProviderBancoDados.PostgreSQL,
    StringConexao = "Host=localhost;Database=provavida;User Id=postgres;Password=senha123"
};

builder.Services.AdicionarInfraestrutura(configDb);
```

### 3️⃣ Usar SQL Server

```csharp
var configDb = new ConfiguracaoBancoDados
{
    Tipo = TipoProviderBancoDados.SqlServer,
    StringConexao = "Server=localhost;Database=ProvaVida;User Id=sa;Password=..."
};

builder.Services.AdicionarInfraestrutura(configDb);
```

### 4️⃣ Configurar via `appsettings.json`

```json
{
  "BancoDados": {
    "Tipo": "PostgreSQL",
    "StringConexao": "Host=localhost;Database=provavida;User Id=postgres;Password=senha"
  }
}
```

Uso no `Program.cs`:

```csharp
var configDb = builder.Configuration.GetSection("BancoDados").Get<ConfiguracaoBancoDados>();
builder.Services.AdicionarInfraestrutura(configDb);
```

## 🔐 Por que isso funciona?

### ✅ Razões

1. **Repositórios usam interfaces**
   ```csharp
   public class RepositorioUsuario : RepositorioBase<Usuario>, IRepositorioUsuario
   ```
   → Não sabem qual BD está sendo usado

2. **DbContext é agnóstico**
   ```csharp
   // DbContext.OnConfiguring() não tem hardcode de provider
   ```

3. **Factory centraliza a lógica**
   ```csharp
   switch (configuracao.Tipo)
   {
       case TipoProviderBancoDados.SQLite:
           opcoes.UseSqlite(stringConexao);
           break;
       case TipoProviderBancoDados.PostgreSQL:
           opcoes.UseNpgsql(stringConexao);
           break;
   }
   ```

## 📦 Provedores Instalados

| Provedor | Pacote | Status |
|----------|--------|--------|
| SQLite | `Microsoft.EntityFrameworkCore.Sqlite` | ✅ |
| PostgreSQL | `Npgsql.EntityFrameworkCore.PostgreSQL` | ✅ |
| SQL Server | `Microsoft.EntityFrameworkCore.SqlServer` | ⏳ (pronto) |

## 🚀 Migrações para Múltiplos BDs

Cada provedor pode ter suas próprias migrações:

```bash
# Migração para SQLite
dotnet ef migrations add InitialSqlite --context ProvaVidaDbContext

# Migração para PostgreSQL (sem afetar SQLite)
dotnet ef migrations add InitialPostgres --context ProvaVidaDbContext

# Aplicar migrações
dotnet ef database update --context ProvaVidaDbContext
```

## 🎯 Resumo

| Aspecto | Como Funciona |
|---------|--------------|
| **Trocar BD** | Apenas mude `TipoProviderBancoDados` |
| **Código dos Repositórios** | Não muda nada |
| **DbContext** | Reutiliza mesma classe |
| **Lógica de Negócio** | Totalmente isolada |
| **Configuração** | Centralizada em um lugar |

---

**Conclusão**: Você pode adicionar PostgreSQL, SQL Server, MySQL ou qualquer outro provedor suportado pelo EF Core **sem tocar em nenhuma linha de código existente**. A arquitetura foi desenhada para ser **extensível e flexível**! 🚀
