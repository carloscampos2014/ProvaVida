---
inclusion: always
---

# Arquitetura e Padrões — ProvaVida

Referência rápida para toda sessão de implementação. Detalhes completos em `docs/Arquitetura.md`.

---

## Stack

| Componente | Tecnologia |
|------------|------------|
| Runtime | .NET 10 |
| Mobile | .NET MAUI — "Enzojb Prova de Vida" |
| API | ASP.NET Core Web API |
| Admin | Blazor Server — Basic Auth — SSH Tunnel porta 5019 |
| BD Servidor | PostgreSQL — banco `provavida` |
| BD Mobile | SQLite — `FileSystem.AppDataDirectory` |
| Acesso dados | Dapper + DbUp (SEM Entity Framework, SEM AutoMapper) |
| Doc API | Scalar (`/scalar`) |
| Testes | xUnit + FluentAssertions + Moq + Bogus |

---

## Estrutura de Projetos (Clean Architecture)

```
src/
  Api/     Domain → Application → Infrastructure → Web
  Mobile/  Domain → Application → Infrastructure → App
  Admin/   Application → Infrastructure → Web
  Shared/  (sem dependências — usado por todos)
tests/
  Api.Tests / Mobile.Tests / Admin.Tests
```

Regra: dependências sempre apontam para dentro. Domain nunca importa nada externo.

---

## Padrões Obrigatórios

- **1 classe por arquivo `.cs`** — nome do arquivo = nome da classe
- **XML docs** em todas as interfaces e classes públicas (`/// <summary>`)
- **Result / Result\<T\>** para retorno de use cases — propriedades `Success` e `MessageErro`
- **CQRS simples** — `IXCommandService` (escrita) e `IXQueryService` (leitura) — sem MediatR
- **FluentValidation** para validação de input — validators em classes separadas
- **Notifications** para agregar múltiplos erros sem lançar exceção
- **ILogger\<T\>** injetado — `LogError` para exceções, `LogWarning` para falhas esperadas
- **Mapeamentos** via métodos de extensão `.ToDto()` / `.ToEntity()` — AutoMapper proibido
- **ViewModel Mobile** nunca acessa `IDbConnection` ou repositório diretamente

---

## Autenticação

- Hash SHA-256 gerado **no app mobile** antes de enviar à API
- Hash salvo no SQLite local e no PostgreSQL — API nunca vê senha em texto puro
- **"Logado"** = existe registro de `Usuario` no SQLite — JWT é só transporte
- JWT de curta duração + refresh token; fallback: login silencioso com hash do SQLite
- Se conta deletada no servidor: sincronismo limpa SQLite → redireciona para Login

---

## Notificações

| Gatilho | Canal | Destinatário |
|---------|-------|--------------|
| 20h — 1 dia sem check-in | Push (FCM/APNs) | Usuário |
| 21h — 2º dia sem check-in | Push (FCM/APNs) | Usuário |
| 3º dia sem check-in | E-mail + WhatsApp + SMS (paralelo) → voz (fallback) | Contato de emergência |

Twilio para WhatsApp, SMS e voz. Gmail SMTP para e-mail.

---

## Repositório e Banco

- `IRepository<T>` e `DapperRepository<T>` em `Shared`
- Implementações concretas: `PostgresXRepository` (Api/Admin) e `SqliteXRepository` (Mobile)
- DbUp executa migrations no startup — scripts em `Infrastructure/Migrations/`
- Scripts PostgreSQL e SQLite separados (tipos de dados divergem)

---

## Deploy

- GitHub Actions dispara no push em `master`
- Build + testes + publish + SCP para `/opt/provavida/api/` na VM Oracle
- VM: Ubuntu 24.04, .NET 10 runtime, PostgreSQL 16, service `provavida-api` porta 5001
- Secrets: `SSH_KEY`, `SSH_HOST`, `SSH_PORT`, `SSH_USER`, `DB_CONNECTION_STRING`, `JWT_SECRET`

---

## Git Flow

```
master ← dev-refatoracao ← feature/fase-N-descricao
```

- Branch por task: `feature/fase-N-descricao`
- PR feature → `dev-refatoracao`: revisão obrigatória
- PR fase → `master`: revisão + deploy automático
- Commits: `tipo(escopo): #N descrição`
- Tasks com tela: E2E manual obrigatório antes do PR

---

## Referência de Layout

Para telas do Mobile e Admin, consultar branch `origin/backup` — manter consistência visual sem redesenhar do zero.
