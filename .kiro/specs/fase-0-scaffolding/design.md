# Design — Fase 0: Scaffolding

## Estrutura de Pastas e Projetos

```
ProvaVida.slnx
├── src/
│   ├── Api/
│   │   ├── ProvaVida.Api.Domain/
│   │   │   └── ProvaVida.Api.Domain.csproj
│   │   ├── ProvaVida.Api.Application/
│   │   │   └── ProvaVida.Api.Application.csproj
│   │   ├── ProvaVida.Api.Infrastructure/
│   │   │   ├── Migrations/
│   │   │   │   ├── V001__criar_tabela_usuarios.sql
│   │   │   │   └── V002__criar_tabela_checkins.sql
│   │   │   ├── Repositories/
│   │   │   │   ├── PostgresUsuarioRepository.cs
│   │   │   │   └── PostgresCheckinRepository.cs
│   │   │   ├── Data/
│   │   │   │   └── DatabaseMigrator.cs
│   │   │   └── ProvaVida.Api.Infrastructure.csproj
│   │   └── ProvaVida.Api.Web/
│   │       ├── appsettings.json
│   │       ├── appsettings.Development.json
│   │       ├── Program.cs
│   │       └── ProvaVida.Api.Web.csproj
│   ├── Mobile/
│   │   ├── ProvaVida.Mobile.Domain/
│   │   │   └── ProvaVida.Mobile.Domain.csproj
│   │   ├── ProvaVida.Mobile.Application/
│   │   │   └── ProvaVida.Mobile.Application.csproj
│   │   ├── ProvaVida.Mobile.Infrastructure/
│   │   │   ├── Migrations/
│   │   │   │   ├── V001__criar_tabela_usuarios.sql
│   │   │   │   └── V002__criar_tabela_checkins.sql
│   │   │   ├── Repositories/
│   │   │   │   ├── SqliteUsuarioRepository.cs
│   │   │   │   └── SqliteCheckinRepository.cs
│   │   │   ├── Data/
│   │   │   │   └── DatabaseMigrator.cs
│   │   │   └── ProvaVida.Mobile.Infrastructure.csproj
│   │   └── ProvaVida.Mobile.App/
│   │       ├── MauiProgram.cs
│   │       ├── App.xaml / App.xaml.cs
│   │       └── ProvaVida.Mobile.App.csproj
│   ├── Admin/
│   │   ├── ProvaVida.Admin.Application/
│   │   │   └── ProvaVida.Admin.Application.csproj
│   │   ├── ProvaVida.Admin.Infrastructure/
│   │   │   ├── Repositories/
│   │   │   │   └── AdminUsuarioRepository.cs
│   │   │   └── ProvaVida.Admin.Infrastructure.csproj
│   │   └── ProvaVida.Admin.Web/
│   │       ├── Program.cs
│   │       ├── appsettings.json
│   │       └── ProvaVida.Admin.Web.csproj
│   └── Shared/
│       ├── Common/
│       │   ├── Result.cs
│       │   └── ResultT.cs
│       ├── Repositories/
│       │   ├── IRepository.cs
│       │   ├── DapperRepository.cs
│       │   ├── IUsuarioRepository.cs
│       │   └── ICheckinRepository.cs
│       └── ProvaVida.Shared.csproj
├── tests/
│   ├── ProvaVida.Api.Tests/
│   │   └── ProvaVida.Api.Tests.csproj
│   ├── ProvaVida.Mobile.Tests/
│   │   └── ProvaVida.Mobile.Tests.csproj
│   └── ProvaVida.Admin.Tests/
│       └── ProvaVida.Admin.Tests.csproj
└── .github/
    └── workflows/
        ├── ci.yml
        └── deploy-api.yml
```

---

## Decisões de Design

### D-001 — `.slnx` em vez de `.sln`
O novo formato XML do Visual Studio 2022+ é mais limpo e legível. Suportado a partir do VS 2022 17.10+.

### D-002 — `ProvaVida.Shared` sem sufixo de camada
O Shared é transversal — não pertence a Domain, Application nem Infrastructure de nenhum componente específico. Contém contratos que cruzam fronteiras (Result, IRepository, DTOs de API request/response futuramente).

### D-003 — Migrations em arquivos SQL separados por banco
Scripts SQLite e PostgreSQL têm diferenças pontuais (tipos de dados, constraints). Manter separados evita condicionais no código. Nomenclatura `V001__descricao.sql` garante ordem de execução determinística via DbUp.

### D-004 — `DatabaseMigrator` como serviço de inicialização
Classe responsável por configurar e executar o DbUp no startup. Registrada no DI e executada via `IHostedService` de inicialização. Isso garante que as migrations rodem antes de qualquer request.

### D-005 — Result Pattern via herança simples
`Result<T>` herda de `Result` para evitar duplicação das propriedades `Success` e `MessageErro`. Métodos estáticos de fábrica em ambas as classes. Sem dependências externas.

### D-006 — Repositórios Admin separados dos da API
O Admin acessa o banco diretamente (leitura e operações administrativas) sem passar pela API. Ter repositórios próprios em `Admin.Infrastructure` permite queries específicas de admin (agregações, relatórios) sem poluir os repositórios da API.

### D-007 — Basic Auth no Admin via middleware customizado
Blazor Server não tem suporte nativo a HTTP Basic Auth. Implementado via `AuthenticationMiddleware` customizado que lê o header `Authorization: Basic <base64>` e valida contra as variáveis de ambiente `ADMIN_USUARIO` e `ADMIN_SENHA`.

### D-008 — CI separado do deploy
`ci.yml` roda em todo push/PR — build + testes. `deploy-api.yml` roda apenas no merge em `master` — build + testes + publish + SCP + restart. Separação garante feedback rápido no desenvolvimento sem disparar deploy acidental.

---

## Referências entre Projetos

```
ProvaVida.Shared
  └── (sem referências)

ProvaVida.Api.Domain
  └── ProvaVida.Shared

ProvaVida.Api.Application
  ├── ProvaVida.Api.Domain
  └── ProvaVida.Shared

ProvaVida.Api.Infrastructure
  ├── ProvaVida.Api.Application
  ├── ProvaVida.Api.Domain
  └── ProvaVida.Shared

ProvaVida.Api.Web
  ├── ProvaVida.Api.Application
  └── ProvaVida.Api.Infrastructure  ← apenas para registro no DI

ProvaVida.Mobile.Domain
  └── ProvaVida.Shared

ProvaVida.Mobile.Application
  ├── ProvaVida.Mobile.Domain
  └── ProvaVida.Shared

ProvaVida.Mobile.Infrastructure
  ├── ProvaVida.Mobile.Application
  ├── ProvaVida.Mobile.Domain
  └── ProvaVida.Shared

ProvaVida.Mobile.App
  ├── ProvaVida.Mobile.Application
  └── ProvaVida.Mobile.Infrastructure  ← apenas para registro no DI

ProvaVida.Admin.Application
  └── ProvaVida.Shared

ProvaVida.Admin.Infrastructure
  ├── ProvaVida.Admin.Application
  └── ProvaVida.Shared

ProvaVida.Admin.Web
  ├── ProvaVida.Admin.Application
  └── ProvaVida.Admin.Infrastructure  ← apenas para registro no DI

ProvaVida.Api.Tests
  ├── ProvaVida.Api.Application
  ├── ProvaVida.Api.Domain
  └── ProvaVida.Shared

ProvaVida.Mobile.Tests
  ├── ProvaVida.Mobile.Application
  ├── ProvaVida.Mobile.Domain
  └── ProvaVida.Shared

ProvaVida.Admin.Tests
  ├── ProvaVida.Admin.Application
  └── ProvaVida.Shared
```

---

## Schema SQL inicial

### PostgreSQL (`provavida`)

```sql
-- V001__criar_tabela_usuarios.sql
CREATE TABLE IF NOT EXISTS usuarios (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    nome VARCHAR(200) NOT NULL,
    email VARCHAR(200) NOT NULL UNIQUE,
    whatsapp VARCHAR(20) NOT NULL,
    senha_hash VARCHAR(64) NOT NULL,
    contato_emergencia_nome VARCHAR(200) NOT NULL,
    contato_emergencia_email VARCHAR(200) NOT NULL,
    contato_emergencia_whatsapp VARCHAR(20) NOT NULL,
    criado_em TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    atualizado_em TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

-- V002__criar_tabela_checkins.sql
CREATE TABLE IF NOT EXISTS checkins (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    usuario_id UUID NOT NULL REFERENCES usuarios(id) ON DELETE CASCADE,
    data DATE NOT NULL,
    latitude DOUBLE PRECISION NOT NULL,
    longitude DOUBLE PRECISION NOT NULL,
    identificacao_aparelho VARCHAR(200) NOT NULL,
    sincronizado BOOLEAN NOT NULL DEFAULT FALSE,
    criado_em TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    CONSTRAINT uq_checkin_usuario_data UNIQUE (usuario_id, data)
);
```

### SQLite (Mobile)

```sql
-- V001__criar_tabela_usuarios.sql
CREATE TABLE IF NOT EXISTS usuarios (
    id TEXT PRIMARY KEY,
    nome TEXT NOT NULL,
    email TEXT NOT NULL UNIQUE,
    whatsapp TEXT NOT NULL,
    senha_hash TEXT NOT NULL,
    contato_emergencia_nome TEXT NOT NULL,
    contato_emergencia_email TEXT NOT NULL,
    contato_emergencia_whatsapp TEXT NOT NULL,
    criado_em TEXT NOT NULL,
    atualizado_em TEXT NOT NULL
);

-- V002__criar_tabela_checkins.sql
CREATE TABLE IF NOT EXISTS checkins (
    id TEXT PRIMARY KEY,
    usuario_id TEXT NOT NULL REFERENCES usuarios(id),
    data TEXT NOT NULL,
    latitude REAL NOT NULL,
    longitude REAL NOT NULL,
    identificacao_aparelho TEXT NOT NULL,
    sincronizado INTEGER NOT NULL DEFAULT 0,
    criado_em TEXT NOT NULL,
    UNIQUE(usuario_id, data)
);
```

---

## NuGet Packages por projeto

| Projeto | Packages |
|---------|---------|
| `Api.Infrastructure` | `Dapper`, `Npgsql`, `DbUp-PostgreSQL` |
| `Api.Web` | `Scalar.AspNetCore`, `Microsoft.AspNetCore.Authentication.JwtBearer` |
| `Mobile.Infrastructure` | `Dapper`, `Microsoft.Data.Sqlite`, `DbUp-SQLite` |
| `Admin.Infrastructure` | `Dapper`, `Npgsql` |
| `Admin.Web` | (Blazor Server built-in) |
| `*.Tests` | `xunit`, `FluentAssertions`, `Moq`, `Bogus` |

---

## GitHub Actions — deploy-api.yml (esboço)

```yaml
name: Deploy API
on:
  push:
    branches: [master]
jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - run: dotnet build --configuration Release
      - run: dotnet test --no-build --configuration Release
      - run: dotnet publish src/Api/ProvaVida.Api.Web/ProvaVida.Api.Web.csproj
               -c Release -o ./publish
      - name: Deploy to VM
        # SCP binários + SSH restart serviço
        # Usa secrets: SSH_KEY, SSH_HOST, SSH_PORT, SSH_USER
```
