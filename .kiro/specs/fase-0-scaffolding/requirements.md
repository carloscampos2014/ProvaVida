# Requirements — Fase 0: Scaffolding

## Objetivo

Criar a estrutura completa da solução `.slnx` com todos os projetos, referências entre camadas, configuração de DI, migrations iniciais, repository base com Dapper e workflow de CI/CD via GitHub Actions. Ao final desta fase, a solução deve compilar, os testes devem rodar e o deploy automatizado deve funcionar — sem nenhuma funcionalidade de negócio ainda.

---

## Requisitos Funcionais

### RF-01 — Solução e Projetos
- A solução deve ser um arquivo `ProvaVida.slnx` na raiz do repositório
- Devem existir os seguintes projetos `.csproj`, todos targeting `net10.0`:

| Projeto | Tipo | Camada |
|---------|------|--------|
| `ProvaVida.Shared` | Class Library | Shared |
| `ProvaVida.Api.Domain` | Class Library | Domain (API) |
| `ProvaVida.Api.Application` | Class Library | Application (API) |
| `ProvaVida.Api.Infrastructure` | Class Library | Infrastructure (API) |
| `ProvaVida.Api.Web` | ASP.NET Core Web API | Presentation (API) |
| `ProvaVida.Mobile.Domain` | Class Library | Domain (Mobile) |
| `ProvaVida.Mobile.Application` | Class Library | Application (Mobile) |
| `ProvaVida.Mobile.Infrastructure` | Class Library | Infrastructure (Mobile) |
| `ProvaVida.Mobile.App` | .NET MAUI | Presentation (Mobile) |
| `ProvaVida.Admin.Application` | Class Library | Application (Admin) |
| `ProvaVida.Admin.Infrastructure` | Class Library | Infrastructure (Admin) |
| `ProvaVida.Admin.Web` | Blazor Server | Presentation (Admin) |
| `ProvaVida.Api.Tests` | xUnit Test Project | Testes API |
| `ProvaVida.Mobile.Tests` | xUnit Test Project | Testes Mobile |
| `ProvaVida.Admin.Tests` | xUnit Test Project | Testes Admin |

### RF-02 — Referências entre projetos (Clean Architecture)
- As referências entre projetos devem respeitar a regra de dependência da Clean Architecture
- Nenhuma camada interna pode referenciar camada externa
- `Domain` não referencia nenhum projeto
- `Application` referencia apenas `Domain` e `Shared`
- `Infrastructure` referencia `Application` e `Domain`
- `Presentation` (Web/App) referencia apenas `Application`
- Projetos de `Shared` podem ser referenciados por qualquer camada
- Projetos de `Tests` referenciam o projeto que testam + bibliotecas de teste

### RF-03 — Migrations iniciais (DbUp)
- A `Api.Infrastructure` deve ter DbUp configurado com PostgreSQL
- Deve existir o script de migration inicial criando as tabelas `usuarios` e `checkins` no banco `provavida`
- A `Mobile.Infrastructure` deve ter DbUp configurado com SQLite
- Deve existir o script de migration inicial criando as tabelas `usuarios` e `checkins` no SQLite local
- As migrations devem executar automaticamente na inicialização de cada entry point

### RF-04 — Repository base (Dapper)
- O projeto `Shared` deve conter a interface `IRepository<T>` genérica
- O projeto `Shared` deve conter a classe abstrata `DapperRepository<T>` com implementação base
- Deve existir `IUsuarioRepository` e `ICheckinRepository` no `Shared`
- Implementações concretas PostgreSQL em `Api.Infrastructure`
- Implementações concretas SQLite em `Mobile.Infrastructure`
- Implementações concretas PostgreSQL (leitura) em `Admin.Infrastructure`

### RF-05 — Result Pattern
- O projeto `Shared` deve conter a classe `Result` (sem dado)
- O projeto `Shared` deve conter a classe `Result<T>` (com dado)
- Ambas com propriedades `Success` (bool) e `MessageErro` (string?)
- Métodos estáticos `Ok()` / `Ok(T data)` e `Fail(string erro)`

### RF-06 — Injeção de Dependência
- `Api.Web` deve ter DI configurado para:
  - `IDbConnection` → `NpgsqlConnection` (PostgreSQL)
  - Repositórios PostgreSQL
  - DbUp executando migrations no startup
- `Mobile.App` deve ter DI configurado para:
  - `IDbConnection` → `SqliteConnection` (SQLite local)
  - Repositórios SQLite
  - DbUp executando migrations no startup
- `Admin.Web` deve ter DI configurado para:
  - `IDbConnection` → `NpgsqlConnection` (PostgreSQL)
  - Repositórios Admin PostgreSQL
  - DbUp executando migrations no startup

### RF-07 — GitHub Actions CI/CD
- Deve existir workflow `.github/workflows/deploy-api.yml`
- Trigger: push em `master`
- Passos: checkout → setup .NET 10 → build → testes → publish → SCP para VM → restart serviço
- Deve existir workflow `.github/workflows/ci.yml`
- Trigger: push em qualquer branch e PRs
- Passos: checkout → setup .NET 10 → build → testes

### RF-08 — Configuração base da API
- `Api.Web` deve ter Scalar configurado (`/scalar`) para documentação
- `Api.Web` deve ter configuração de JWT (middleware, validação)
- `Api.Web` deve ter `appsettings.json` e `appsettings.Development.json`
- Configurações sensíveis via variáveis de ambiente (nunca em arquivos commitados)

### RF-09 — Configuração base do Admin
- `Admin.Web` deve ter HTTP Basic Auth configurado
- `Admin.Web` deve ter página inicial em branco (placeholder)
- Porta configurada para `5019` (localhost only)

---

## Requisitos Não Funcionais

- RNF-01: Todos os projetos targeting `net10.0`
- RNF-02: Uma classe por arquivo `.cs`
- RNF-03: XML docs em todas as interfaces e classes públicas do `Shared`
- RNF-04: Sem AutoMapper — mapeamentos via métodos de extensão
- RNF-05: Solução deve compilar sem warnings após scaffolding
- RNF-06: Todos os testes (mesmo que placeholder) devem passar no `dotnet test`
- RNF-07: Secrets nunca em arquivos commitados — apenas via variáveis de ambiente / GitHub Secrets

---

## Fora do Escopo desta Fase

- Nenhuma funcionalidade de negócio (login, check-in, etc.)
- Telas do app Mobile (apenas estrutura de projetos)
- Telas do Admin (apenas estrutura + placeholder)
- Workers de background
- Notificações
