# Tasks — Fase 0: Scaffolding

> Cada task é uma issue no GitHub com label `fase-0-scaffolding`.
> Branch: `feature/fase-0-<descricao>` a partir de `dev-refatoracao`.
> Critério de conclusão: build passando + testes passando + PR aprovado.

---

- [ ] **Task 0.1 — Criar solução ProvaVida.slnx e estrutura de pastas**

  Branch: `feature/fase-0-criar-solucao-slnx`

  - Criar o arquivo `ProvaVida.slnx` na raiz do repositório
  - Criar a estrutura de pastas: `src/Api/`, `src/Mobile/`, `src/Admin/`, `src/Shared/`, `tests/`
  - Criar todos os 15 projetos `.csproj` conforme tabela do design
  - Adicionar todos os projetos ao `.slnx`
  - Todos os projetos targeting `net10.0` (MAUI: `net10.0-android;net10.0-ios;net10.0-windows10.0.19041.0`)
  - Critério: `dotnet build ProvaVida.slnx` conclui sem erros

---

- [ ] **Task 0.2 — Configurar referências entre projetos (Clean Architecture)**

  Branch: `feature/fase-0-referencias-clean-architecture`

  - Configurar `<ProjectReference>` em cada `.csproj` conforme diagrama de dependências do design
  - `Domain` sem referências de projeto
  - `Application` referencia apenas `Domain` e `Shared`
  - `Infrastructure` referencia `Application` e `Domain`
  - `Web/App` referencia `Application` e `Infrastructure` (apenas para DI)
  - Critério: `dotnet build` passa, nenhuma referência circular

---

- [ ] **Task 0.3 — Implementar Result e Result\<T\> no Shared**

  Branch: `feature/fase-0-result-pattern`

  - Criar `src/Shared/Common/Result.cs` com `Success`, `MessageErro`, `Ok()`, `Fail(string)`
  - Criar `src/Shared/Common/ResultT.cs` herdando de `Result` com `Data`, `Ok(T)`, `Fail(string)`
  - XML docs em ambas as classes
  - Testes TDD: `Result_Ok_DeveRetornarSuccessTrue`, `Result_Fail_DeveRetornarSuccessFalseComMensagem`, `ResultT_Ok_DeveRetornarSuccessTrueComDado`, `ResultT_Fail_DeveRetornarSuccessFalseSemDado`
  - Critério: 4 testes passando, `dotnet build` passa

---

- [ ] **Task 0.4 — Implementar IRepository\<T\> e DapperRepository\<T\> no Shared**

  Branch: `feature/fase-0-repository-base`

  - Criar `IRepository.cs` com `GetByIdAsync`, `GetAllAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`
  - Criar `DapperRepository.cs` como classe abstrata base com Dapper
  - Criar `IUsuarioRepository.cs` e `ICheckinRepository.cs` (interfaces vazias por ora)
  - XML docs em todas as interfaces e classe base
  - Critério: `dotnet build` passa, teste de contrato passando

---

- [ ] **Task 0.5 — Configurar DbUp + migrations iniciais para API (PostgreSQL)**

  Branch: `feature/fase-0-dbup-api-postgresql`

  - Adicionar NuGet `DbUp-PostgreSQL` em `Api.Infrastructure`
  - Criar `DatabaseMigrator.cs` em `Api.Infrastructure/Data/`
  - Criar scripts `V001__criar_tabela_usuarios.sql` e `V002__criar_tabela_checkins.sql` como `EmbeddedResource`
  - Registrar execução das migrations no startup da API
  - Critério: `dotnet build` passa, migrations executam no startup

---

- [ ] **Task 0.6 — Configurar DbUp + migrations iniciais para Mobile (SQLite)**

  Branch: `feature/fase-0-dbup-mobile-sqlite`

  - Adicionar NuGet `DbUp-SQLite` e `Microsoft.Data.Sqlite` em `Mobile.Infrastructure`
  - Criar `DatabaseMigrator.cs` em `Mobile.Infrastructure/Data/`
  - Criar scripts `V001__criar_tabela_usuarios.sql` e `V002__criar_tabela_checkins.sql` como `EmbeddedResource`
  - Caminho do SQLite via `FileSystem.AppDataDirectory`
  - Registrar execução no startup do MAUI
  - Critério: `dotnet build` passa para o projeto Mobile

---

- [ ] **Task 0.7 — Implementar repositórios concretos PostgreSQL (API)**

  Branch: `feature/fase-0-repositorios-postgres-api`

  - Criar entidades `Usuario` e `Checkin` em `Api.Domain` (POCOs puros)
  - Criar `PostgresUsuarioRepository.cs` implementando `IUsuarioRepository`, herdando `DapperRepository<Usuario>`
  - Criar `PostgresCheckinRepository.cs` implementando `ICheckinRepository`, herdando `DapperRepository<Checkin>`
  - Teste TDD: `PostgresUsuarioRepository_GetByIdAsync_DeveRetornarNull_QuandoNaoEncontrado`
  - Critério: testes passando, `dotnet build` passa

---

- [ ] **Task 0.8 — Implementar repositórios concretos SQLite (Mobile)**

  Branch: `feature/fase-0-repositorios-sqlite-mobile`

  - Criar entidades `Usuario` e `Checkin` em `Mobile.Domain` (POCOs puros)
  - Criar `SqliteUsuarioRepository.cs` e `SqliteCheckinRepository.cs`
  - Teste TDD: `SqliteUsuarioRepository_GetByIdAsync_DeveRetornarNull_QuandoNaoEncontrado`
  - Critério: teste passando, `dotnet build` passa

---

- [ ] **Task 0.9 — Implementar repositórios Admin (PostgreSQL direto)**

  Branch: `feature/fase-0-repositorios-admin`

  - Criar `AdminUsuarioRepository.cs` implementando `IUsuarioRepository`, herdando `DapperRepository<Usuario>`
  - Por enquanto apenas `GetAllAsync` e `GetByIdAsync`
  - XML docs
  - Critério: `dotnet build` passa

---

- [ ] **Task 0.10 — Configurar DI e startup da API**

  Branch: `feature/fase-0-di-startup-api`

  - Configurar `Program.cs`: `IDbConnection` → `NpgsqlConnection`, repositórios, DbUp, Scalar (`/scalar`), JWT middleware
  - `appsettings.json` e `appsettings.Development.json` sem secrets (apenas via env var)
  - Criar `global.json` na raiz com `sdk.version: "10.0"`
  - Critério: API sobe sem erro, `/scalar` acessível, `dotnet build` passa

---

- [ ] **Task 0.11 — Configurar DI, resources e startup do Mobile (MAUI)**

  Branch: `feature/fase-0-di-startup-mobile`

  - Configurar `MauiProgram.cs`: `IDbConnection` → `SqliteConnection`, repositórios, DbUp
  - Nome do app: **"Enzojb Prova de Vida"** (`ApplicationTitle` e `DisplayName` no `.csproj`)
  - Copiar resources de `docs/resources/`: `appicon.svg` e `appiconfg.svg` → `AppIcon/`, `splash.svg` → `Splash/`, `Colors.xaml` e `Styles.xaml` → `Styles/`
  - Tela inicial placeholder (`ContentPage` vazio)
  - **E2E Manual (Windows):** app abre sem crash, banco SQLite criado com tabelas `usuarios` e `checkins`
  - Critério: E2E aprovado, `dotnet build` passa para target Windows

---

- [ ] **Task 0.12 — Configurar DI, Basic Auth e startup do Admin**

  Branch: `feature/fase-0-di-startup-admin`

  - Configurar `Program.cs`: `IDbConnection` → `NpgsqlConnection`, repositórios, HTTP Basic Auth (valida `ADMIN_USUARIO` e `ADMIN_SENHA` env vars), porta `5019`
  - Página inicial Blazor placeholder: "ProvaVida Admin — Em construção"
  - **E2E Manual:** browser pede credenciais → corretas abrem placeholder → erradas retornam 401
  - Critério: E2E aprovado, `dotnet build` passa

---

- [ ] **Task 0.13 — Criar workflow CI (GitHub Actions)**

  Branch: `feature/fase-0-github-actions-ci`

  - Criar `.github/workflows/ci.yml`
  - Trigger: `push` em qualquer branch + `pull_request`
  - Passos: checkout → setup .NET 10 → restore → build → test
  - Excluir projetos MAUI do build do CI (requer workloads específicos)
  - Critério: CI passa no GitHub, build e testes verdes

---

- [ ] **Task 0.14 — Criar workflow Deploy API (GitHub Actions)**

  Branch: `feature/fase-0-github-actions-deploy`

  - Criar `.github/workflows/deploy-api.yml`
  - Trigger: `push` em `master`
  - Passos: checkout → setup .NET 10 → restore → build → test → publish → SCP para VM → restart `provavida-api` → health check
  - Usa secrets: `SSH_KEY`, `SSH_HOST`, `SSH_PORT`, `SSH_USER`
  - Critério: deploy funciona após merge em `master`, API respondendo na VM

---

> **Ordem recomendada:** 0.1 → 0.2 → 0.3 → 0.4 → (0.5 ∥ 0.6) → (0.7 ∥ 0.8) → 0.9 → 0.10 → 0.11 → 0.12 → (0.13 ∥ 0.14)
