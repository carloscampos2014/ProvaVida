# Arquitetura — ProvaVida

> Documento de referência técnica do projeto. Toda decisão de implementação deve ser consistente com as definições aqui registradas.

---

## 1. Visão Geral

```
┌──────────────────────────────────────────────────────────┐
│                    App Mobile (MAUI)                     │
│  SQLite local · Background services · Online preferencial│
└───────────────────────┬──────────────────────────────────┘
                        │ HTTPS · JWT
                        ▼
┌──────────────────────────────────────────────────────────┐
│                  API (ASP.NET Core)                      │
│  REST · JWT Auth · Background Workers · PostgreSQL       │
└───────────────────────┬──────────────────────────────────┘
                        │ Conexão direta
                        ▼
┌──────────────────────────────────────────────────────────┐
│                     PostgreSQL                           │
└───────────────────────┬──────────────────────────────────┘
                        │ Conexão direta (SSH Tunnel)
                        ▼
┌──────────────────────────────────────────────────────────┐
│              Painel Admin (Blazor Server)                │
│  Basic Auth · Acesso exclusivo via SSH Tunnel            │
└──────────────────────────────────────────────────────────┘
```

---

## 2. Stack Tecnológica

| Componente | Tecnologia | Observação |
|------------|------------|------------|
| Mobile | .NET MAUI (C#) | Android e iOS — online preferencial, fallback offline |
| API | ASP.NET Core (C#) | REST API |
| Admin | Blazor Server (C#) | Painel interno |
| BD Servidor | PostgreSQL | API e Admin |
| BD Mobile | SQLite | Armazenamento local no dispositivo |
| Acesso a dados | Dapper | Micro-ORM, SQL explícito |
| Migrations | DbUp | Scripts SQL versionados |
| Auth Mobile→API | JWT | Access token de curta duração |
| Auth Admin | HTTP Basic Auth | Acesso via SSH Tunnel |
| Testes | xUnit + Moq | TDD |
| Workflow | SDD (Spec Driven Development) | Spec → aprovação → implementação |

---

## 3. Estrutura de Projetos

```
ProvaVida.slnx
├── src/
│   ├── Api/
│   │   ├── ProvaVida.Api.Domain          # Entidades, interfaces, regras puras
│   │   ├── ProvaVida.Api.Application     # Use cases, DTOs, contratos de serviço
│   │   ├── ProvaVida.Api.Infrastructure  # Repositórios Dapper, workers, serviços externos
│   │   └── ProvaVida.Api.Web             # Entry point: Controllers, middlewares, DI
│   ├── Mobile/
│   │   ├── ProvaVida.Mobile.Domain       # Entidades, interfaces, regras puras
│   │   ├── ProvaVida.Mobile.Application  # Use cases, DTOs
│   │   ├── ProvaVida.Mobile.Infrastructure # Repositórios Dapper/SQLite, background services
│   │   └── ProvaVida.Mobile.App          # Entry point: Pages, ViewModels, DI (MAUI)
│   ├── Admin/
│   │   ├── ProvaVida.Admin.Application   # Use cases, queries de leitura
│   │   ├── ProvaVida.Admin.Infrastructure # Repositórios Dapper/PostgreSQL direto
│   │   └── ProvaVida.Admin.Web           # Entry point: Blazor Pages, componentes, DI
│   └── Shared/
│       └── ProvaVida.Shared              # DTOs de request/response da API, contratos cross-cutting
└── tests/
    ├── ProvaVida.Api.Tests
    ├── ProvaVida.Mobile.Tests
    └── ProvaVida.Admin.Tests
```

---

## 4. Clean Architecture — Regras de Dependência

As dependências sempre apontam para dentro. Camadas externas dependem de internas, nunca o contrário.

```
Presentation → Application → Domain
Infrastructure → Application → Domain
```

| Camada | Pode depender de | Não pode depender de |
|--------|-----------------|----------------------|
| Domain | nada | Application, Infrastructure, Presentation |
| Application | Domain | Infrastructure, Presentation |
| Infrastructure | Application, Domain | Presentation |
| Presentation | Application | Infrastructure (direto) |

**Regra prática:** o `Domain` nunca importa nada de fora. O `Application` define interfaces (`IUserRepository`) que o `Infrastructure` implementa — inversão de dependência via DI.

---

## 5. Modelo de Dados

As entidades são espelhadas entre SQLite (Mobile) e PostgreSQL (API/Admin). As classes de domínio são POCOs puros — sem atributos de ORM.

### Usuario

| Campo | Tipo | Observação |
|-------|------|------------|
| Id | GUID | Chave primária |
| Nome | string | |
| Email | string | Único |
| Whatsapp | string | |
| SenhaHash | string | Hash gerado no app mobile antes de enviar |
| ContatoEmergenciaNome | string | |
| ContatoEmergenciaEmail | string | |
| ContatoEmergenciaWhatsapp | string | |
| CriadoEm | DateTime | UTC |
| AtualizadoEm | DateTime | UTC |

### Checkin

| Campo | Tipo | Observação |
|-------|------|------------|
| Id | GUID | Chave primária |
| UsuarioId | GUID | FK para Usuario |
| Data | DateOnly | Apenas a data — um por dia |
| Latitude | double | |
| Longitude | double | |
| IdentificacaoAparelho | string | Device ID |
| Sincronizado | bool | Flag de sincronismo |
| CriadoEm | DateTime | UTC |

### Heartbeat (somente servidor)

| Campo | Tipo | Observação |
|-------|------|------------|
| Id | GUID | Chave primária |
| UsuarioId | GUID | FK para Usuario |
| RecebidoEm | DateTime | UTC |

---

## 6. Autenticação

### Regra de sessão no Mobile

**"Está logado"** = existe registro de `Usuario` na tabela local do SQLite.  
**"Não está logado"** = SQLite não tem registro de `Usuario`.

O app **nunca redireciona para Login** enquanto houver dados de usuário no SQLite — mesmo que o token JWT tenha expirado.

### Hash de senha

- O hash é gerado **no app mobile** (SHA-256) antes de qualquer chamada à API.
- O mesmo hash é salvo no **SQLite local** e enviado/armazenado no **PostgreSQL**.
- A API **nunca recebe a senha em texto puro** — apenas o hash.
- O hash salvo no SQLite é utilizado para renovação silenciosa do token JWT.

### Fluxo JWT

```
1. Login:
   App → POST /auth/login { email, senhaHash }
   API → { accessToken, refreshToken, expiresAt }
   App salva tokens no Secure Storage do SO (Keychain/Keystore)

2. Chamada normal (token válido):
   App verifica expiresAt → ainda válido
   App → [endpoint] Authorization: Bearer <accessToken>

3. Renovação silenciosa (token expirado):
   App verifica expiresAt → expirado
   App → POST /auth/refresh { refreshToken }
   API → { novoAccessToken, novoExpiresAt }
   App → [endpoint] Authorization: Bearer <novoAccessToken>

4. Refresh falho (token inválido/revogado):
   App → POST /auth/refresh → 401
   App faz novo login silencioso usando hash do SQLite
   App → POST /auth/login { email, senhaHash do SQLite }

5. Login silencioso também falha (conta deletada no servidor):
   Sincronismo detecta conta excluída → limpa SQLite
   App redireciona para tela de Login
```

### Autenticação do Admin

- HTTP Basic Auth com credencial única de administrador.
- O painel só é acessível via SSH Tunnel — não exposto publicamente.
- Não compartilha autenticação com o sistema de usuários do app.

---

## 7. Repository Pattern

### Interface genérica (em Shared ou Domain)

```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}
```

### Classe base com Dapper

- `DapperRepository<T>` — implementação base no `Shared` ou em cada `Infrastructure`.
- Recebe `IDbConnection` via injeção de dependência.
- SQL básico (SELECT, INSERT, UPDATE, DELETE) é comum entre os dois bancos.
- Divergências pontuais (retorno de ID, funções de data, upsert) ficam nas implementações concretas.

### Implementações concretas

| Implementação | Projeto | Banco |
|---------------|---------|-------|
| `PostgresUserRepository` | Api.Infrastructure | PostgreSQL |
| `PostgresCheckinRepository` | Api.Infrastructure | PostgreSQL |
| `SqliteUserRepository` | Mobile.Infrastructure | SQLite |
| `SqliteCheckinRepository` | Mobile.Infrastructure | SQLite |
| `AdminUserRepository` | Admin.Infrastructure | PostgreSQL (direto) |

### Registro no DI

```csharp
// API
services.AddScoped<IDbConnection>(_ => new NpgsqlConnection(connStr));
services.AddScoped<IUserRepository, PostgresUserRepository>();

// Mobile
services.AddScoped<IDbConnection>(_ => new SqliteConnection(dbPath));
services.AddScoped<IUserRepository, SqliteUserRepository>();

// Admin
services.AddScoped<IDbConnection>(_ => new NpgsqlConnection(connStr));
services.AddScoped<IUserRepository, AdminUserRepository>();
```

---

## 8. Migrations

### API e Admin — DbUp + PostgreSQL

- Scripts SQL versionados em `src/Api/ProvaVida.Api.Infrastructure/Migrations/`.
- Nomenclatura: `V001__criar_tabela_usuarios.sql`, `V002__criar_tabela_checkins.sql`, etc.
- DbUp executa na inicialização da API e do Admin — só scripts ainda não executados.
- Scripts são imutáveis após aplicados em produção.

### Mobile — SQLite

- Scripts SQL versionados em `src/Mobile/ProvaVida.Mobile.Infrastructure/Migrations/`.
- Mesma estratégia DbUp, adaptada para `Microsoft.Data.Sqlite`.
- Executado na inicialização do app (Fluxo de Inicialização).

---

## 9. Background Services (Workers)

Todos implementados como `IHostedService` / `BackgroundService` no .NET.

| Worker | Localização | Frequência | Responsabilidade |
|--------|-------------|------------|-----------------|
| HeartbeatWorker | Mobile.Infrastructure | A cada 3h | Envia sinal de vida ao servidor |
| SyncWorker | Mobile.Infrastructure | A cada 1h | Sincroniza dados com o servidor |
| NotificacaoPushUsuarioWorker | Api.Infrastructure | Agendado (20h) | Push notification para usuários sem check-in no dia |
| NotificacaoPushEmergenciaWorker | Api.Infrastructure | Agendado (21h D+2) | Push notification para usuários sem check-in por 2 dias seguidos |
| NotificacaoMulticanalEmergenciaWorker | Api.Infrastructure | Agendado (D+3) | E-mail + WhatsApp + SMS simultâneos ao contato de emergência; fallback ligação de voz |

### Lógica de notificações

**Notificações push (servidor → celular do usuário):**
- Disparadas pelo servidor via push notification (FCM/APNs)
- Acionadas pelos workers agendados nos horários definidos

**Notificações ao próprio usuário (1 dia sem check-in):**
```
20h: Push notification no celular do usuário → fim.
```

**Notificações ao contato de emergência (2 dias seguidos sem check-in):**
```
Passo 1 (21h do 2º dia): Push notification no celular do usuário
Passo 2 (3º dia, se ainda sem check-in): E-mail + WhatsApp + SMS simultâneos ao contato de emergência
Passo 3 (se todos os três falharem): Ligação de voz para o contato de emergência
```

E-mail, WhatsApp e SMS são **disparados em paralelo** — não são tentativas sequenciais. A ligação de voz é o fallback apenas quando os três falham simultaneamente.

---

## 10. Painel Administrativo

- **Stack:** Blazor Server (C#)
- **Autenticação:** HTTP Basic Auth — credencial única de admin
- **Acesso:** exclusivamente via SSH Tunnel (não exposto à internet)
- **Banco:** acessa PostgreSQL diretamente via `Admin.Infrastructure` (sem passar pela API)
- **Funcionalidades:**
  - Usuários cadastrados
  - Usuários com check-in atrasado
  - Notificações enviadas aos usuários
  - Notificações enviadas aos contatos de emergência
  - Histórico de check-ins
  - Teste manual de envio (e-mail, WhatsApp, SMS, ligação de voz)
  - Controle de backup do banco PostgreSQL

---

## 11. Padrões de Desenvolvimento

| Princípio | Aplicação |
|-----------|-----------|
| **SOLID** | Uma responsabilidade por classe, interfaces para abstrações, DI para inversão de dependência |
| **KISS** | Sem abstrações desnecessárias — resolver o problema mais simples que funciona |
| **Clean Code** | Nomes expressivos, métodos pequenos, sem comentários redundantes |
| **Clean Architecture** | Dependências apontam para o domínio; domínio sem referências externas |
| **TDD** | Teste falha primeiro (Red) → implementação mínima (Green) → refactor |
| **SDD** | Spec (requirements → design → tasks) → aprovação → implementação task a task |

### Workflow SDD por módulo

```
1. requirements.md  → O que o módulo faz (regras de negócio)
2. design.md        → Como será implementado (estrutura, decisões)
3. tasks.md         → Lista de tasks atômicas e verificáveis
4. Aprovação        → Usuário aprova antes de qualquer código
5. Implementação    → Task a task, com testes (TDD)
```

Os specs ficam em `.kiro/specs/<nome-do-modulo>/`.
