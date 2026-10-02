# Design Document

## Overview

A Fase 2 implementa autenticação completa no app Mobile MAUI seguindo Clean Architecture. ViewModels chamam use cases em `Mobile.Application`, que coordenam repositórios SQLite em `Mobile.Infrastructure` e chamadas HTTP à API via `ApiClient`. O SQLite é a fonte de verdade do estado de sessão — "logado" significa que existe um registro de `Usuario` no banco local.

## Architecture

A autenticação mobile é organizada em camadas:

```
Mobile.App (MAUI)
  Pages: LoginPage, CadastroPage
  ViewModels: LoginViewModel, CadastroViewModel
      ↓
Mobile.Application
  Auth: IAuthMobileService, ISessionManager, ITokenManager
  Utils: Sha256Helper
  Validators: CadastrarUsuarioMobileValidator, LoginMobileValidator
      ↓
Mobile.Infrastructure
  Http: IApiClient, ApiClient → API REST
  Repositories: ITokenRepository, SqliteTokenRepository → SQLite
      ↓
Shared
  Repositories: IUsuarioRepository, SqliteUsuarioRepository → SQLite
```

Regras de dependência (Clean Architecture):
- `Mobile.App` referencia apenas `Mobile.Application`
- `Mobile.Application` referencia apenas `Shared` — nunca `Infrastructure` diretamente
- `Mobile.Infrastructure` referencia `Mobile.Application` e `Shared`
- ViewModels nunca acessam `IDbConnection` ou repositórios diretamente

### Fluxo de Renovação de Token

```
GetAccessTokenAsync()
  ├── token válido?          → retornar access token
  ├── token expirado         → RefreshTokenAsync()
  │     ├── refresh válido?  → novos tokens → salvar SQLite → retornar
  │     └── refresh inválido → SilentLoginAsync()
  │           ├── hash no SQLite? → POST /auth/login → novos tokens → salvar → retornar
  │           └── falha           → ClearSession() → navegar para Login
  └── retornar token renovado
```

## Components and Interfaces

### `Sha256Helper` — `Mobile.Application/Utils/`

```csharp
public static class Sha256Helper
{
    /// <summary>Computa o hash SHA-256 da senha em texto puro.</summary>
    public static string ComputeHash(string senha): string
}
```

### `IApiClient` / `ApiClient` — `Mobile.Infrastructure/Http/`

```csharp
public interface IApiClient
{
    Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest body);
}
```

`ApiClient` usa `IHttpClientFactory` com base URL configurável via `appsettings` por ambiente.

### `ITokenRepository` / `SqliteTokenRepository` — `Mobile.Infrastructure/Repositories/`

```csharp
public interface ITokenRepository
{
    Task SaveAsync(TokenLocal token);
    Task<TokenLocal?> GetByUsuarioIdAsync(string usuarioId);
    Task DeleteByUsuarioIdAsync(string usuarioId);
}
```

### `ISessionManager` / `SessionManager` — `Mobile.Application/Auth/`

```csharp
public interface ISessionManager
{
    Task<bool> IsLoggedInAsync();
    Task<Usuario?> GetCurrentUserAsync();
    Task SaveSessionAsync(Usuario usuario, string senhaHash);
    Task ClearSessionAsync();  // exclui usuario + checkins do SQLite
}
```

### `ITokenManager` / `TokenManager` — `Mobile.Application/Auth/`

```csharp
public interface ITokenManager
{
    Task<string> GetAccessTokenAsync();   // renova silenciosamente se necessário
    Task<bool> RefreshTokenAsync();
    Task<bool> SilentLoginAsync();
}
```

### `IAuthMobileService` / `AuthMobileService` — `Mobile.Application/Auth/`

```csharp
public interface IAuthMobileService
{
    Task<Result> RegisterAsync(RegisterRequest request);
    Task<Result> LoginAsync(string email, string senha);
    Task<Result> LogoutAsync();
}
```

`LoginAsync` gera hash SHA-256 da senha antes de chamar a API.

### ViewModels — `Mobile.App/ViewModels/`

```csharp
// Padrão CommunityToolkit.Mvvm
public partial class LoginViewModel : ObservableObject
{
    [ObservableProperty] string email;
    [ObservableProperty] string senha;
    [ObservableProperty] bool isLoading;
    [ObservableProperty] string mensagemErro;

    [RelayCommand] async Task LoginAsync()
    [RelayCommand] async Task IrParaCadastroAsync()
}
```

`CadastroViewModel` segue o mesmo padrão com campos adicionais de cadastro e contato de emergência.

## Data Models

### Migration SQLite — `V003__adicionar_colunas_auth.sql`

```sql
ALTER TABLE usuarios ADD COLUMN senha_hash TEXT NOT NULL DEFAULT '';

CREATE TABLE IF NOT EXISTS tokens (
    id            TEXT PRIMARY KEY,
    usuario_id    TEXT NOT NULL,
    access_token  TEXT NOT NULL,
    refresh_token TEXT NOT NULL,
    expires_at    TEXT NOT NULL,
    senha_hash    TEXT NOT NULL,
    criado_em     TEXT NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
);
```

### `TokenLocal` — entidade local para persistência de tokens

| Campo | Tipo | Descrição |
|---|---|---|
| `Id` | `string` | GUID do registro |
| `UsuarioId` | `string` | FK para `usuarios.id` |
| `AccessToken` | `string` | JWT atual |
| `RefreshToken` | `string` | Refresh token atual |
| `ExpiresAt` | `DateTime` | Expiração do access token |
| `SenhaHash` | `string` | Hash SHA-256 para login silencioso |
| `CriadoEm` | `DateTime` | Data de criação |

### Base URL por Ambiente

| Ambiente | Base URL |
|---|---|
| Development (WSL2) | `http://localhost:5001` |
| Produção (VM Oracle) | Configurado via `appsettings.json` / env var |

## Correctness Properties

- A senha em texto puro NUNCA deve ser persistida nem trafegar pela rede — apenas o hash SHA-256
- "Logado" é determinado exclusivamente pela existência de `Usuario` no SQLite — não pelo estado do JWT
- `ClearSessionAsync` deve sempre excluir tanto `usuarios` quanto `checkins` do SQLite (conforme `Fluxo-Logoff.md`)
- O hash SHA-256 deve ser salvo no SQLite apenas após login bem-sucedido na API
- ViewModels nunca devem injetar ou chamar repositórios diretamente

## Error Handling

| Cenário | Tratamento |
|---|---|
| Sem conectividade no login/cadastro | Exibir mensagem de erro e não enviar requisição |
| Falha na criação do banco SQLite | Exibir erro crítico e encerrar o app |
| API retorna 401 no refresh | Tentar login silencioso com hash |
| Login silencioso falha | `ClearSession` + navegar para Login |
| API retorna erro de validação | Exibir mensagem do servidor ao usuário |
| Conta deletada no servidor | `ClearSession` + navegar para Login |

## Testing Strategy

- **Testes unitários** (`Mobile.Tests`): `AuthMobileService`, `SessionManager`, `TokenManager`, `Sha256Helper`, validators — usando Moq para `IApiClient`, `IUsuarioRepository` e `ITokenRepository`
- **Testes de integração** (`Mobile.IntegrationTests`): `SqliteTokenRepository` com SQLite in-memory
- **E2E Manual (Windows)**: Task 8 requer validação visual antes do PR — fluxos: inicialização sem usuário → Login; cadastro → Login; login → Check-in; logoff → Login
- Cobertura obrigatória: todos os 4 caminhos do `TokenManager` (token válido, refresh, login silencioso, falha)
