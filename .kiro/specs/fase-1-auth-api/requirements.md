# Requirements — Fase 1: Autenticação (API)

## Objetivo

Implementar a camada de autenticação na API do ProvaVida (`ProvaVida.Api`), permitindo cadastro de usuários, login enviando hash de senha, emissão e validação de access token (JWT), suporte e rotação de refresh token, e invalidação de token (logout).

---

## Requisitos Funcionais

### RF-101 — Cadastro de Usuário (`POST /auth/register`)
- A API deve expor o endpoint publicamente `POST /auth/register`.
- O endpoint deve receber: nome, email, whatsapp, senhaHash (SHA-256), contatoEmergenciaNome, contatoEmergenciaEmail, contatoEmergenciaWhatsapp.
- O e-mail deve ser único na base de dados.
- Caso o e-mail já esteja cadastrado, deve retornar falha explicativa (`400 Bad Request` ou `409 Conflict`).
- Deve validar os campos obrigatórios e formatos de e-mail e telefone usando `FluentValidation`.
- Ao cadastrar com sucesso, deve retornar o usuário criado sem expor a senhaHash.

### RF-102 — Login (`POST /auth/login`)
- A API deve expor o endpoint publicamente `POST /auth/login`.
- O endpoint deve receber e-mail e senhaHash.
- A API deve comparar o e-mail e a senhaHash informada com os dados salvos na tabela `usuarios`.
- Se as credenciais forem válidas:
  - Gerar um Access Token JWT com tempo de expiração curto (ex: 15 a 60 minutos).
  - Gerar um Refresh Token seguro (randômico, criptograficamente forte) com validade mais longa (ex: 7 a 30 dias).
  - Persistir o Refresh Token associado ao usuário.
  - Retornar o Access Token, Refresh Token, tipo ("Bearer") e data de expiração (`expiresAt`).
- Se as credenciais forem inválidas, deve retornar `401 Unauthorized`.

### RF-103 — Renovação de Token (`POST /auth/refresh`)
- A API deve expor o endpoint publicamente `POST /auth/refresh`.
- O endpoint deve receber o Refresh Token atual.
- Se o Refresh Token for válido, ativo e não expirado:
  - Revogar / invalidar o Refresh Token antigo.
  - Gerar um novo Access Token JWT e um novo Refresh Token (Refresh Token Rotation).
  - Salvar o novo Refresh Token na base de dados.
  - Retornar os novos tokens.
- Se o Refresh Token for inválido, revogado ou expirado, deve retornar `401 Unauthorized`.

### RF-104 — Logout (`POST /auth/logout`)
- A API deve expor o endpoint protegido `POST /auth/logout` (requer JWT Bearer válido).
- Deve receber o Refresh Token a ser revogado.
- Deve marcar o Refresh Token como revogado na base de dados.
- Retornar resposta de sucesso (`200 OK` ou `204 No Content`).

### RF-105 — Middleware de Autenticação JWT
- Rotas protegidas na API devem exigir a presença do header `Authorization: Bearer <accessToken>`.
- O middleware deve validar a assinatura do JWT, emissor, audiência e data de expiração.
- Se inválido ou ausente em rota protegida, deve retornar `401 Unauthorized`.

---

## Requisitos Não Funcionais

- RNF-101: A API **nunca** deve receber a senha do usuário em texto puro, apenas a hash SHA-256 gerada previamente pelo cliente.
- RNF-102: Os tokens JWT devem ser assinados via chave simétrica HMAC-SHA256 obtida via variável de ambiente / configuração (`Jwt:Secret`).
- RNF-103: Todas as respostas de erro de validação ou de negócio devem seguir o `Result` / `Result<T>` pattern do `ProvaVida.Shared`.
- RNF-104: Cobertura de testes unitários para Use Cases, Validators e Serviços de Autenticação com 100% dos fluxos principais testados.
