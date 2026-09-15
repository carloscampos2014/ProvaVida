# Design — Fase 1: Autenticação (API)

## Arquitetura e Estrutura de Componentes

As alterações e adições de código da Fase 1 seguirão estritamente a Clean Architecture configurada no monorepo:

```
src/
├── Shared/
│   └── ProvaVida.Shared/
│       ├── Dtos/
│       │   ├── RegisterRequest.cs
│       │   ├── LoginRequest.cs
│       │   ├── RefreshTokenRequest.cs
│       │   ├── LogoutRequest.cs
│       │   └── TokenResponse.cs
│       └── Entities/
│           ├── Usuario.cs
│           └── RefreshToken.cs
├── Api/
│   ├── ProvaVida.Api.Domain/
│   │   └── Interfaces/
│   │       ├── ITokenService.cs
│   │       └── IRefreshTokenRepository.cs
│   ├── ProvaVida.Api.Application/
│   │   ├── Commands/
│   │   │   ├── CadastrarUsuarioCommand.cs
│   │   │   ├── LoginCommand.cs
│   │   │   ├── RefreshTokenCommand.cs
│   │   │   └── LogoutCommand.cs
│   │   ├── Validators/
│   │   │   ├── CadastrarUsuarioValidator.cs
│   │   │   └── LoginValidator.cs
│   │   └── Services/
│   │       └── AuthApplicationService.cs
│   ├── ProvaVida.Api.Infrastructure/
│   │   ├── Migrations/
│   │   │   └── V003__criar_tabela_refresh_tokens.sql
│   │   ├── Services/
│   │   │   └── JwtTokenService.cs
│   │   └── Repositories/
│   │       └── PostgresRefreshTokenRepository.cs
│   └── ProvaVida.Api.Web/
│       └── Controllers/
│           └── AuthController.cs
```

---

## Esquema do Banco de Dados (PostgreSQL)

### Migration `V003__criar_tabela_refresh_tokens.sql`

```sql
CREATE TABLE IF NOT EXISTS refresh_tokens (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    usuario_id UUID NOT NULL REFERENCES usuarios(id) ON DELETE CASCADE,
    token VARCHAR(500) NOT NULL UNIQUE,
    expira_em TIMESTAMP WITH TIME ZONE NOT NULL,
    revogado BOOLEAN NOT NULL DEFAULT FALSE,
    criado_em TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    revogado_em TIMESTAMP WITH TIME ZONE NULL
);

CREATE INDEX IF NOT EXISTS idx_refresh_tokens_token ON refresh_tokens(token);
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_usuario_id ON refresh_tokens(usuario_id);
```

---

## Contratos DTOs (`ProvaVida.Shared`)

### `RegisterRequest`
```csharp
public record RegisterRequest(
    string Nome,
    string Email,
    string Whatsapp,
    string SenhaHash,
    string ContatoEmergenciaNome,
    string ContatoEmergenciaEmail,
    string ContatoEmergenciaWhatsapp
);
```

### `LoginRequest`
```csharp
public record LoginRequest(
    string Email,
    string SenhaHash
);
```

### `RefreshTokenRequest`
```csharp
public record RefreshTokenRequest(
    string RefreshToken
);
```

### `TokenResponse`
```csharp
public record TokenResponse(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    int ExpiresInSeconds
);
```

---

## Contrato do Controller (`AuthController`)

```csharp
[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request);

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request);

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request);

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request);
}
```

---

## Fluxo de Autenticação e Rotação de Tokens

1. **Login (`POST /auth/login`):**
   - Valida e-mail e `senhaHash` recebidos contra a tabela `usuarios`.
   - Se válidos, gera um Access Token JWT com `UsuarioId`, `Email` e `Nome` nas claims (expiração: 60 minutos).
   - Gera um `RefreshToken` criptograficamente aleatório (string Base64 de 64 bytes).
   - Salva o `RefreshToken` no banco com `expira_em = NOW() + 7 dias` e `revogado = false`.
   - Retorna o `TokenResponse`.

2. **Refresh (`POST /auth/refresh`):**
   - Busca o `RefreshToken` no banco de dados.
   - Verifica se existe, se `revogado == false` e se `expira_em > NOW()`.
   - Se válido, marca o token atual como `revogado = true` e `revogado_em = NOW()`.
   - Gera um novo Access Token JWT e um novo `RefreshToken` (Refresh Token Rotation).
   - Salva o novo token na tabela `refresh_tokens`.
   - Retorna o novo `TokenResponse`.

3. **Logout (`POST /auth/logout`):**
   - Marca o `RefreshToken` enviado no corpo como `revogado = true` e `revogado_em = NOW()`.
