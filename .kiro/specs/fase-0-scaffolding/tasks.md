# Tasks — Fase 0: Scaffolding

> Cada task é uma issue no GitHub com label `fase-0-scaffolding`.
> Branch: `feature/fase-0-<descricao>` a partir de `dev-refatoracao`.
> Critério de conclusão: build passando + testes passando + PR aprovado.

---

## Task 0.1 — Criar solução ProvaVida.slnx e estrutura de pastas

**Branch:** `feature/fase-0-criar-solucao-slnx`

### O que fazer
- Criar o arquivo `ProvaVida.slnx` na raiz do repositório
- Criar a estrutura de pastas: `src/Api/`, `src/Mobile/`, `src/Admin/`, `src/Shared/`, `tests/`
- Criar todos os 15 projetos `.csproj` conforme tabela do design
- Adicionar todos os projetos ao `.slnx`
- Garantir que `dotnet build` conclui sem erros em todos os projetos

### Critério de aceite
- [ ] `ProvaVida.slnx` existe na raiz
- [ ] 15 projetos criados e referenciados na solução
- [ ] Todos targeting `net10.0`
- [ ] `dotnet build ProvaVida.slnx` conclui sem erros
- [ ] Projetos MAUI targeting `net10.0-android;net10.0-ios;net10.0-windows10.0.19041.0`

---

## Task 0.2 — Configurar referências entre projetos (Clean Architecture)

**Branch:** `feature/fase-0-referencias-clean-architecture`

### O que fazer
- Configurar `<ProjectReference>` em cada `.csproj` conforme diagrama de dependências do design
- Validar que nenhuma camada interna referencia camada externa
- `Domain` sem referências de projeto
- `Application` referencia apenas `Domain` e `Shared`
- `Infrastructure` referencia `Application` e `Domain`
- `Web/App` referencia `Application` e `Infrastructure` (apenas para DI)

### Critério de aceite
- [ ] `dotnet build` passa em toda a solução após configurar referências
- [ ] Nenhuma referência circular
- [ ] Nenhum projeto interno referenciando camada externa

---

## Task 0.3 — Implementar Result e Result\<T\> no Shared

**Branch:** `feature/fase-0-result-pattern`

### O que fazer
- Criar `src/Shared/Common/Result.cs` — classe base com `Success` e `MessageErro`
- Criar `src/Shared/Common/ResultT.cs` — `Result<T>` herdando de `Result` com propriedade `Data`
- Métodos estáticos `Ok()`, `Fail(string)` em `Result`
- Métodos estáticos `Ok(T)`, `Fail(string)` em `Result<T>`
- XML docs em ambas as classes

### Testes (TDD)
- `Result_Ok_DeveRetornarSuccessTrue`
- `Result_Fail_DeveRetornarSuccessFalseComMensagem`
- `ResultT_Ok_DeveRetornarSuccessTrueComDado`
- `ResultT_Fail_DeveRetornarSuccessFalseSemDado`

### Critério de aceite
- [ ] Classes criadas com XML docs
- [ ] 4 testes passando em `ProvaVida.Api.Tests`
- [ ] `dotnet build` passa

---

## Task 0.4 — Implementar IRepository\<T\> e DapperRepository\<T\> no Shared

**Branch:** `feature/fase-0-repository-base`

### O que fazer
- Criar `src/Shared/Repositories/IRepository.cs` com interface genérica (`GetByIdAsync`, `GetAllAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`)
- Criar `src/Shared/Repositories/DapperRepository.cs` com classe abstrata base usando Dapper
- Criar `src/Shared/Repositories/IUsuarioRepository.cs` (interface vazia por ora — será expandida na Fase 1)
- Criar `src/Shared/Repositories/ICheckinRepository.cs` (interface vazia por ora — será expandida na Fase 3)
- XML docs em todas as interfaces e classe base

### Testes (TDD)
- `IRepository_InterfaceDeveConterMetodosEsperados` (via reflexão ou implementação mock)

### Critério de aceite
- [ ] Interfaces e classe base criadas com XML docs
- [ ] `dotnet build` passa
- [ ] Teste passando

---

## Task 0.5 — Configurar DbUp + migrations iniciais para API (PostgreSQL)

**Branch:** `feature/fase-0-dbup-api-postgresql`

### O que fazer
- Adicionar NuGet `DbUp-PostgreSQL` em `Api.Infrastructure`
- Criar `src/Api/ProvaVida.Api.Infrastructure/Data/DatabaseMigrator.cs`
- Criar scripts SQL em `src/Api/ProvaVida.Api.Infrastructure/Migrations/`:
  - `V001__criar_tabela_usuarios.sql`
  - `V002__criar_tabela_checkins.sql`
- Scripts devem ser `EmbeddedResource` no `.csproj`
- `DatabaseMigrator` deve executar DbUp apontando para o banco `provavida`
- Registrar execução das migrations no startup da API

### Critério de aceite
- [ ] Scripts SQL criados conforme schema do design
- [ ] `DatabaseMigrator` implementado
- [ ] Migrations executam no startup sem erro (testável localmente ou via integração)
- [ ] `dotnet build` passa

---

## Task 0.6 — Configurar DbUp + migrations iniciais para Mobile (SQLite)

**Branch:** `feature/fase-0-dbup-mobile-sqlite`

### O que fazer
- Adicionar NuGet `DbUp-SQLite` e `Microsoft.Data.Sqlite` em `Mobile.Infrastructure`
- Criar `src/Mobile/ProvaVida.Mobile.Infrastructure/Data/DatabaseMigrator.cs`
- Criar scripts SQL em `src/Mobile/ProvaVida.Mobile.Infrastructure/Migrations/`:
  - `V001__criar_tabela_usuarios.sql`
  - `V002__criar_tabela_checkins.sql`
- Scripts devem ser `EmbeddedResource` no `.csproj`
- Caminho do SQLite: pasta `AppData` do dispositivo (`FileSystem.AppDataDirectory`)
- Registrar execução no startup do MAUI (`MauiProgram.cs`)

### Critério de aceite
- [ ] Scripts SQL criados conforme schema SQLite do design
- [ ] `DatabaseMigrator` implementado para SQLite
- [ ] `dotnet build` passa para o projeto Mobile

---

## Task 0.7 — Implementar repositórios concretos PostgreSQL (API)

**Branch:** `feature/fase-0-repositorios-postgres-api`

### O que fazer
- Criar `src/Api/ProvaVida.Api.Infrastructure/Repositories/PostgresUsuarioRepository.cs`
  - Implementa `IUsuarioRepository`
  - Herda de `DapperRepository<Usuario>`
  - Métodos base funcionando (CRUD via Dapper + SQL explícito)
- Criar `src/Api/ProvaVida.Api.Infrastructure/Repositories/PostgresCheckinRepository.cs`
  - Implementa `ICheckinRepository`
  - Herda de `DapperRepository<Checkin>`
- Criar entidades `Usuario` e `Checkin` em `Api.Domain` (POCOs puros, sem atributos de ORM)

### Testes (TDD)
- Testes unitários com `Moq` para repositório (mock do `IDbConnection`)
- `PostgresUsuarioRepository_GetByIdAsync_DeveRetornarNull_QuandoNaoEncontrado`

### Critério de aceite
- [ ] Repositórios criados com XML docs
- [ ] Entidades criadas em `Api.Domain`
- [ ] Testes passando
- [ ] `dotnet build` passa

---

## Task 0.8 — Implementar repositórios concretos SQLite (Mobile)

**Branch:** `feature/fase-0-repositorios-sqlite-mobile`

### O que fazer
- Criar `src/Mobile/ProvaVida.Mobile.Infrastructure/Repositories/SqliteUsuarioRepository.cs`
- Criar `src/Mobile/ProvaVida.Mobile.Infrastructure/Repositories/SqliteCheckinRepository.cs`
- Criar entidades `Usuario` e `Checkin` em `Mobile.Domain` (POCOs puros)
- Mesma estrutura dos repositórios PostgreSQL, adaptada para SQLite

### Testes (TDD)
- `SqliteUsuarioRepository_GetByIdAsync_DeveRetornarNull_QuandoNaoEncontrado`

### Critério de aceite
- [ ] Repositórios criados
- [ ] Entidades em `Mobile.Domain`
- [ ] Teste passando
- [ ] `dotnet build` passa

---

## Task 0.9 — Implementar repositórios Admin (PostgreSQL direto)

**Branch:** `feature/fase-0-repositorios-admin`

### O que fazer
- Criar `src/Admin/ProvaVida.Admin.Infrastructure/Repositories/AdminUsuarioRepository.cs`
  - Implementa `IUsuarioRepository` do Shared
  - Herda de `DapperRepository<Usuario>`
  - Por enquanto apenas `GetAllAsync` e `GetByIdAsync`

### Critério de aceite
- [ ] Repositório criado com XML docs
- [ ] `dotnet build` passa

---

## Task 0.10 — Configurar DI e startup da API

**Branch:** `feature/fase-0-di-startup-api`

### O que fazer
- Configurar `Program.cs` da `Api.Web` com:
  - `IDbConnection` → `NpgsqlConnection` (connection string via `DB_CONNECTION_STRING` env var)
  - Repositórios PostgreSQL registrados
  - DbUp migrations no startup
  - Scalar configurado (`/scalar`)
  - JWT middleware configurado (secret via `JWT_SECRET` env var)
  - `appsettings.json` e `appsettings.Development.json` sem secrets
- Configurar `global.json` na raiz apontando `sdk.version: "10.0"`

### Critério de aceite
- [ ] API sobe sem erro (localmente ou com variáveis de ambiente mockadas)
- [ ] `/scalar` acessível
- [ ] `dotnet build` passa

---

## Task 0.11 — Configurar DI e startup do Mobile (MAUI)

**Branch:** `feature/fase-0-di-startup-mobile`

### O que fazer
- Configurar `MauiProgram.cs` com:
  - `IDbConnection` → `SqliteConnection` (caminho via `FileSystem.AppDataDirectory`)
  - Repositórios SQLite registrados
  - DbUp migrations no startup
- App deve iniciar sem crash no target Windows
- Tela inicial placeholder (apenas `ContentPage` vazio com título "ProvaVida")

### E2E Manual
- Rodar no target **Windows Machine**
- Verificar que o app abre sem crash
- Verificar que o banco SQLite é criado na pasta AppData

### Critério de aceite
- [ ] App inicializa no Windows sem crash
- [ ] Banco SQLite criado com tabelas `usuarios` e `checkins`
- [ ] `dotnet build` passa para o target Windows
- [ ] E2E manual aprovado

---

## Task 0.12 — Configurar DI, Basic Auth e startup do Admin

**Branch:** `feature/fase-0-di-startup-admin`

### O que fazer
- Configurar `Program.cs` da `Admin.Web` com:
  - `IDbConnection` → `NpgsqlConnection`
  - Repositórios Admin registrados
  - HTTP Basic Auth middleware (valida `ADMIN_USUARIO` e `ADMIN_SENHA` env vars)
  - Porta configurada para `5019`
- Página inicial Blazor placeholder: "ProvaVida Admin — Em construção"

### E2E Manual
- Rodar localmente
- Verificar que abre a tela de autenticação (browser pede usuário/senha)
- Verificar que credenciais corretas dão acesso à página placeholder
- Verificar que credenciais erradas retornam 401

### Critério de aceite
- [ ] Admin abre com Basic Auth funcionando
- [ ] Página placeholder visível após autenticação
- [ ] E2E manual aprovado
- [ ] `dotnet build` passa

---

## Task 0.13 — Criar workflow CI (GitHub Actions)

**Branch:** `feature/fase-0-github-actions-ci`

### O que fazer
- Criar `.github/workflows/ci.yml`
- Trigger: `push` em qualquer branch + `pull_request`
- Passos:
  1. `actions/checkout@v4`
  2. `actions/setup-dotnet@v4` com `dotnet-version: '10.0.x'`
  3. `dotnet restore`
  4. `dotnet build --no-restore --configuration Release`
  5. `dotnet test --no-build --configuration Release`
- Excluir projetos MAUI do build do CI (build de MAUI requer workloads específicos)

### Critério de aceite
- [ ] Workflow criado
- [ ] CI passa no GitHub após push
- [ ] Build e testes verdes no GitHub Actions

---

## Task 0.14 — Criar workflow Deploy API (GitHub Actions)

**Branch:** `feature/fase-0-github-actions-deploy`

### O que fazer
- Criar `.github/workflows/deploy-api.yml`
- Trigger: `push` em `master`
- Passos:
  1. `actions/checkout@v4`
  2. `actions/setup-dotnet@v4` com `dotnet-version: '10.0.x'`
  3. `dotnet restore`
  4. `dotnet build --no-restore --configuration Release`
  5. `dotnet test --no-build --configuration Release`
  6. `dotnet publish src/Api/ProvaVida.Api.Web/ProvaVida.Api.Web.csproj -c Release -o ./publish`
  7. SCP dos binários para `/opt/provavida/api/` na VM (usando secrets `SSH_KEY`, `SSH_HOST`, `SSH_PORT`, `SSH_USER`)
  8. SSH: `sudo systemctl restart provavida-api`
  9. Health check: aguardar serviço responder

### Critério de aceite
- [ ] Workflow criado
- [ ] Deploy funciona após merge em `master`
- [ ] Serviço `provavida-api` restartado com sucesso na VM
- [ ] API respondendo na VM após deploy

---

## Ordem de execução recomendada

```
0.1 → 0.2 → 0.3 → 0.4 → 0.5 → 0.6 → 0.7 → 0.8 → 0.9 → 0.10 → 0.11 → 0.12 → 0.13 → 0.14
```

Tasks 0.5 e 0.6 podem ser paralelas. Tasks 0.7 e 0.8 podem ser paralelas após 0.4. Tasks 0.13 e 0.14 podem ser feitas a qualquer momento após 0.1.
