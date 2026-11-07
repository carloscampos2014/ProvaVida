# ProvaVida — Status da branch `dev-refatoracao`

Análise estática do código e da documentação de `ProvaVida-dev-refatoracao.zip` (arquivos datados de 02/10/2026) · Gerado em 07/11/2026

## 1. Resumo executivo

A branch tem a **fundação completa (Fase 0)** e a **API de autenticação (Fase 1)**. No mobile existe apenas a infraestrutura (SQLite, hash SHA-256, cliente HTTP): **nenhuma tela de negócio** foi implementada. Check-in, sincronismo, heartbeat, notificações e as telas do painel Admin ainda não existem.

| Indicador | Valor |
| --- | --- |
| Projetos na solução | 17 (12 em `src/`, 5 de teste) |
| Métodos de teste (`[Fact]`/`[Theory]`) | 120 (lidos, não executados) |
| Migrations | 3 PostgreSQL (V001–V003) e 2 SQLite (V001–V002) |
| Endpoints REST | 4 (`/auth/register`, `/auth/login`, `/auth/refresh`, `/auth/logout`) |
| Fases concluídas (cronograma 0–8) | 2 de 9; a Fase 2 está em 1 de 8 tasks |

**Quatro pontos que merecem atenção antes de seguir:**

1. Provável bug que quebra o login fora dos testes: o segredo JWT é lido de duas fontes diferentes (seção 7.1, item A).
2. O design da Fase 2 tem falhas que impedem compilar e migrar (ciclo de referência entre projetos e migration que duplica coluna). Corrigir o spec antes de codar (item D).
3. A Task 9 da Fase 0 está marcada como concluída, mas o `AdminUsuarioRepository` previsto não existe (seção 6).
4. O deploy em `master` só confere a página do Scalar, então não detectaria o problema do item 1 (item A).

**Limites desta análise:** leitura estática. O ambiente não tem o SDK .NET, então não compilei nem rodei os testes. Resultados de E2E manual, issues e PRs do GitHub não estão no zip. A pasta `.kiro/` veio incompleta (seção 6). Se a branch avançou depois de 02/10/2026, este documento pode estar defasado.

## 2. Status por fase

| Fase | Escopo | Tasks no spec | Situação no código |
| --- | --- | --- | --- |
| 0 | Scaffolding | 14/14 marcadas | **Pronta**, com 1 divergência (Task 9) |
| 1 | Auth API | 5/5 | **Pronta**, com 1 bug provável (segredo JWT) e ajustes de segurança |
| 2 | Auth Mobile | 1/8 | **Em andamento**: só `Sha256Helper` e `ApiClient` |
| 3 | Check-in | sem spec | Não iniciada (existem só entidade, tabelas e repositórios base) |
| 4 | Sincronismo e Heartbeat | sem spec | Não iniciada |
| 5 | Gestão de Conta | sem spec | Não iniciada |
| 6 | Push (FCM) | sem spec | Não iniciada |
| 7 | Multicanal (e-mail, WhatsApp, SMS, voz) | sem spec | Não iniciada |
| 8 | Painel Admin | sem spec | Parcial e fora do plano: Basic Auth e consultas de métricas, sem telas |

## 3. O que está pronto e funcionando

### 3.1 Fundação (Fase 0)

- Solução `ProvaVida.slnx` em Clean Architecture: Api (Domain, Application, Infrastructure, Web), Mobile (Domain, Application, Infrastructure, App), Admin (Application, Infrastructure, Web) e `Shared`.
- `Result` e `Result<T>`, `IRepository<T>` e `DapperRepository<T>` (buscar por id, listar, upsert, excluir).
- Repositórios Dapper de `Usuario` e `Checkin` para PostgreSQL (API) e SQLite (Mobile), com type handlers de `Guid`, `DateOnly` e `DateTimeOffset` no SQLite.
- DbUp no PostgreSQL (`EnsureDatabase` mais V001–V003; falha de migration impede o startup) e no SQLite (V001–V002, executado em `MauiProgram`).
- API com DI, JWT Bearer, OpenAPI e Scalar (`/scalar/v1`); connection string via `DB_CONNECTION_STRING`.
- Admin (Blazor Server) na porta fixa 5019 (localhost), Basic Auth via `ADMIN_USUARIO` e `ADMIN_SENHA` (bloqueia tudo se as variáveis não existirem) e página placeholder.
- Mobile (MAUI) com DI, migrations SQLite no startup, nome “Enzojb Prova de Vida” e estilos, ícones e splash vindos de `docs/resources`.
- CI (`ci.yml`): build e testes (unitários e de integração com Testcontainers) em todo push e PR, sem os projetos MAUI. Deploy (`deploy-api.yml`): em push na `master`, publica via SCP, reinicia `provavida-api` e faz health check.

### 3.2 Autenticação na API (Fase 1)

| Endpoint | Acesso | Respostas | Observações |
| --- | --- | --- | --- |
| `POST /auth/register` | público | 201, 400, 409 | FluentValidation; e-mail único; não devolve o hash |
| `POST /auth/login` | público | 200, 401 | compara e-mail e hash; access token JWT (60 min por padrão) e refresh token (7 dias) |
| `POST /auth/refresh` | público | 200, 401 | rotação: revoga o refresh antigo e emite um novo par |
| `POST /auth/logout` | JWT | 204, 400 | revoga o refresh token informado |

Também prontos: tabela `refresh_tokens` com revogação lógica, `JwtTokenService` (HMAC-SHA256; refresh com 64 bytes aleatórios) e validators (WhatsApp com 10–11 dígitos, hash com 64 caracteres).

### 3.3 Mobile (Fase 2, task 1)

`Sha256Helper` (hex minúsculo, 64 caracteres) e `ApiClient` (somente `POST` JSON). Ainda não estão registrados no DI.

### 3.4 Testes (120 métodos)

| Projeto | Métodos | Cobre |
| --- | --- | --- |
| Api.Tests | 57 | Result, type handler, repositórios (mock), JWT (6), AuthApplicationService (16), validators (10) |
| Api.IntegrationTests | 9 | repositórios de Usuario e Checkin em PostgreSQL via Testcontainers |
| Mobile.Tests | 35 | migrator, type handlers, repositórios, SHA-256 |
| Mobile.IntegrationTests | 9 | repositórios SQLite em arquivo temporário |
| Admin.Tests | 10 | consultas de métricas (3 unitários e 7 de integração) |

**Lacunas de teste:** `AuthController` (nenhum teste HTTP), `PostgresRefreshTokenRepository`, `DatabaseMigrator` da API, `BasicAuthMiddleware`, `ApiClient` e a configuração de startup (que teria pego o item A da seção 7).

## 4. Funcionalidades do produto × status

| Funcionalidade (`docs/ProvaVida.md`) | API | Mobile | Observação |
| --- | --- | --- | --- |
| Cadastrar conta | Pronta | Não iniciada | falta a tela (Fase 2) |
| Login | Pronta (ver item A) | Não iniciada | falta tela e sessão |
| Logoff | Pronta (revoga refresh) | Não iniciada | falta limpar SQLite e voltar ao Login |
| Alterar conta | Não iniciada | Não iniciada | Fase 5: `PUT /account` |
| Excluir conta | Não iniciada | Não iniciada | Fase 5: `DELETE /account` |
| Check-in diário | só tabela, entidade e repositório | só tabela, entidade e repositório | sem endpoint, tela, localização nem regra de 1 por dia na API |
| Sincronismo (1h) | Não iniciada | Não iniciada | sem `/sync` e sem worker |
| Heartbeat (3h) | Não iniciada | Não iniciada | nem a tabela `heartbeats` existe |
| Aviso ao usuário (20h e 21h do 2º dia) | Não iniciada | Não iniciada | sem FCM, sem device token, sem workers |
| Aviso ao contato de emergência (3º dia) | Não iniciada | n/a | sem Twilio, sem SMTP, sem workers, sem histórico |
| Painel administrativo | Parcial | n/a | Basic Auth e consultas de métricas; nenhuma tela usa as consultas |

## 5. O que falta implementar

| Fase | Pendências |
| --- | --- |
| 2 | Tasks 2–8: migration V003 do SQLite e `ITokenRepository`; `SessionManager`; `TokenManager`; `AuthMobileService` e validators; `LoginPage`; `CadastroPage`; fluxo de inicialização, DI e E2E. Também: CommunityToolkit.Mvvm (citado no spec, ausente no csproj), serviço de conectividade e tratamento do retorno de `Migrate()`. Pré-requisito: corrigir o design (item D). |
| 3 | `POST /checkins` (1 por dia), tela, captura de localização (falta `ACCESS_FINE_LOCATION` no manifest), check-in offline (`Sincronizado=false`), consultas por usuário/data e por pendentes nos repositórios. |
| 4 | `GET`/`POST /sync`, `POST /heartbeat`, tabela `heartbeats`, SyncWorker (1h), HeartbeatWorker (3h), exclusão de conta detectada pelo sync. Depende da decisão D2. |
| 5 | `PUT` e `DELETE /account`, telas de alterar e excluir conta, logoff completo. |
| 6 | FCM, tabela de device tokens, workers das 20h e 21h (D+2), permissão `POST_NOTIFICATIONS` (Android 13+). Depende da decisão D1. |
| 7 | SMTP (Gmail) e Twilio (WhatsApp, SMS, voz), worker do 3º dia com fallback de ligação, tabela de histórico de notificações. |
| 8 | Telas (usuários, atrasados, notificações, check-ins), testes de envio, controle de backup, remoção do template (Counter, Weather). |
| Fora do cronograma | Trocar o `ApplicationId` do template, revisar `allowBackup`, job Android no CI, endpoint `/health`. |

## 6. Divergências entre documentação/spec e código

| # | Documento ou spec diz | Código tem | Sugestão |
| --- | --- | --- | --- |
| 1 | Fase 0, Task 9 (RF-04 e design): `AdminUsuarioRepository` | só `AdminConnectionFactory` e `AdminMetricasQueryService` (fora do spec) | atualizar o spec (as consultas de leitura combinam com o “CQRS simples” da Arquitetura) ou criar o repositório, e registrar a decisão |
| 2 | Arquitetura §7: `IRepository<T>` com `AddAsync` e `UpdateAsync` | um único `UpsertAsync` | atualizar a Arquitetura (o upsert dá idempotência ao sincronismo) |
| 3 | RF-102 e Arquitetura §6: resposta com `expiresAt` | `TokenResponse` com `ExpiresInSeconds`, fixo em 3600 no serviço, enquanto o gerador do JWT lê `Jwt:ExpiresInMinutes` | escolher um contrato; sugestão: devolver `expiresAt` UTC calculado com o mesmo valor do JWT |
| 4 | RF-105: validar emissor e audiência | `ValidateIssuer` e `ValidateAudience` desligados | definir `iss` e `aud` ou ajustar o requisito |
| 5 | Arquitetura §6: tokens no Secure Storage | design da Fase 2: tabela `tokens` no SQLite | decidir (D3) |
| 6 | Design da Fase 2: URL de desenvolvimento `http://localhost:5001` | `launchSettings.json` da API usa a porta `5268` | alinhar |
| 7 | `Ciclo-Desenvolvimento.md`: secrets `VM_SSH_KEY`, `VM_HOST`, `VM_PORT`, `VM_USER` | workflow e steering usam `SSH_KEY`, `SSH_HOST`, `SSH_PORT`, `SSH_USER` | corrigir a doc |
| 8 | Arquitetura §13: Scalar em desenvolvimento | `MapScalarApiReference` e `MapOpenApi` sem condição de ambiente | restringir (7.2 #5) |
| 9 | Arquitetura §16: Notifications, CQRS (`IXCommandService`/`IXQueryService`), `.ToDto()`/`.ToEntity()` e “nunca engolir exceção sem log” | não aplicados: Auth usa um serviço único, os comandos têm `FromRequest` estático, e `DapperRepository` e as consultas do Admin capturam exceção sem log | aplicar ou revisar a regra |
| 10 | RNF-07 (Fase 0): secrets fora de arquivos commitados | `launchSettings.json` com senha do PostgreSQL e `admin123` | mover para user-secrets |
| 11 | `.slnx` lista `.kiro/hooks`, `knowledge`, `settings`, 5 arquivos de steering e `tasks.meta.json` | esses arquivos não estão no zip, e o `.gitignore` ignora `.kiro/` inteiro, embora specs e steering estejam versionados | decidir o que versionar (liberar `.kiro/specs` e `.kiro/steering` com `!`) e limpar o `.slnx` |
| 12 | Arquitetura.md | seções numeradas fora de ordem (12, 16, 13, 14, 15) | renumerar |
| 13 | n/a | restos de template: `ProvaVida.Api.Web.http` chama `/weatherforecast` (não existe), Admin com `Counter`, `Weather` e `NavMenu` padrão, Mobile com `dotnet_bot.png` | remover |

## 7. Problemas encontrados no código

### 7.1 Prioridade alta

**A. Segredo JWT lido de duas fontes (provável login quebrado).** `Program.cs` valida tokens com a variável `JWT_SECRET` (com fallback para `Jwt:Secret`), mas `JwtTokenService` só lê `Jwt:Secret` da configuração. Uma variável chamada `JWT_SECRET` não vira `Jwt:Secret` (o .NET usa `Jwt__Secret`, com dois underscores, para hierarquia). Nada no repositório define `Jwt__Secret`; o `launchSettings.json` e o steering de deploy só citam `JWT_SECRET`. Efeito esperado: `GenerateAccessToken` lança `InvalidOperationException`, o `AuthApplicationService` captura, e o controller responde 401 “Credenciais inválidas”, escondendo a causa (ela só aparece no log). Os testes passam porque injetam `Jwt:Secret` em memória.

Como confirmar, passo a passo:

1. Com o PostgreSQL local ligado, suba a API pelo perfil `http` (no Visual Studio, ou `dotnet run --launch-profile http` dentro de `src/Api/ProvaVida.Api.Web`). Ela escuta em `http://localhost:5268`.
2. Gere um hash de 64 caracteres: `echo -n "senha123" | sha256sum` (copie só os 64 caracteres, sem o `-`).
3. Abra `http://localhost:5268/scalar/v1`, escolha `POST /auth/register`, preencha os campos (WhatsApp com 10 ou 11 dígitos, `senhaHash` com o hash do passo 2) e envie. Esperado: 201.
4. Em `POST /auth/login`, envie o mesmo e-mail e o mesmo hash. Se o problema existir, a resposta é 401 mesmo com os dados corretos.
5. Olhe o console da API e procure `Jwt:Secret não configurado.` (registrado como erro).
6. Aplique a correção abaixo, reinicie a API e repita o passo 4. Esperado: 200 com `accessToken` e `refreshToken`.

Correção mínima (KISS), em `Program.cs`, logo depois de calcular `jwtSecret`:

```csharp
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["Jwt:Secret"] = jwtSecret // fonte única para validar e gerar tokens
});
```

Evolução: trocar por `IOptions<JwtOptions>` com `ValidateOnStart` (falha no boot, não em runtime); criar um teste de integração com `WebApplicationFactory` (register, login, logout); se mais de um serviço passar a validar tokens, migrar para chave assimétrica. Impacto no ciclo: o PR `dev-refatoracao → master` da Fase 1 dispara deploy automático, e o health check chama apenas `/scalar/v1`. Vale corrigir antes desse PR.

**B. Hash de senha sem salt e sem custo funciona como senha armazenada.** O servidor guarda o SHA-256 recebido e compara com ele (`u.SenhaHash == command.SenhaHash`, comparação comum). Quem obtiver o banco consegue logar enviando o próprio hash, e SHA-256 puro de senha curta é fácil de quebrar. Recomendação: manter o SHA-256 no app (ele permite o login silencioso) e aplicar no servidor um hash lento com salt por usuário sobre o hash recebido (PBKDF2, Argon2 ou BCrypt), com comparação em tempo constante (`CryptographicOperations.FixedTimeEquals`) e limite de tentativas em `/auth/login` (`AddRateLimiter`). Evolução: coluna `hash_versao` para migrar os usuários no próximo login; depois, trocar o SHA-256 do app por PBKDF2 com salt derivado do e-mail.

**C. Login e cadastro carregam todos os usuários.** `AuthApplicationService` chama `GetAllAsync()` e filtra em memória para achar o e-mail. O custo cresce com o número de usuários e os hashes de todos passam pela memória a cada tentativa. Correção: `GetByEmailAsync` no repositório com índice único em `lower(email)` (a violação de unicidade já deve virar “E-mail já cadastrado”). Evolução: expor como `IUsuarioQueryService`, no padrão CQRS da Arquitetura.

**D. Falhas no plano da Fase 2 (corrigir o spec antes de implementar).**

1. *Ciclo de referência.* `ITokenRepository` e `IApiClient` ficam em `Mobile.Infrastructure`, mas são consumidos por `SessionManager`, `TokenManager` e `AuthMobileService`, que ficam em `Mobile.Application`. Como `Infrastructure` já referencia `Application`, o inverso criaria um ciclo. Mover as interfaces para `Mobile.Application` (ou `Mobile.Domain`) e deixar só as implementações na Infrastructure. `IApiClient` (task 1, já feita) precisa ser movida.
2. *Migration V003 falha.* O design faz `ALTER TABLE usuarios ADD COLUMN senha_hash`, mas essa coluna já existe na V001. O SQLite responde “duplicate column name”, e como `MauiProgram` ignora o retorno de `Migrate()`, o app abriria sem banco atualizado.
3. *`ApiClient` perde a mensagem do servidor.* `EnsureSuccessStatusCode()` lança exceção em 400, 401 e 409, mas o RF-202.6 exige exibir a mensagem recebida. Também faltam `GET`, `PUT`, `DELETE` e o cabeçalho `Authorization`. Sugestão: retornar `Result<T>` com status e mensagem e injetar o token por um `DelegatingHandler`.
4. *Tokens e hash em SQLite comum.* Ver decisão D3.

### 7.2 Prioridade média

| # | Onde | Problema | Correção sugerida |
| --- | --- | --- | --- |
| 1 | `SqliteUsuarioRepository` e `SqliteCheckinRepository` | `INSERT OR REPLACE` apaga e reinsere a linha. Em `usuarios`, com chaves estrangeiras ativas (o código não define `PRAGMA foreign_keys`), isso pode apagar em cascata os check-ins ao atualizar o usuário. Em `checkins`, um conflito em `UNIQUE (usuario_id, data)` sobrescreve o check-in anterior em silêncio, em vez de rejeitar a duplicata (regra “um por dia”) | usar `INSERT ... ON CONFLICT(id) DO UPDATE`, como no PostgreSQL, e criar teste de regressão |
| 2 | `MauiProgram` | ignora o retorno de `Migrate()`; falha no banco não exibe erro nem encerra o app (Fluxo-Inicialização, passo 6) | tratar o retorno na Task 8 |
| 3 | `DapperRepository` e `RegisterAsync` | devolve `ex.Message` e o cadastro repassa ao cliente (mensagens internas do banco na resposta 400); sem log | logar a exceção e responder com mensagem genérica |
| 4 | Níveis de log | `LogWarning` para sucessos (login, cadastro, rotação, token gerado) e e-mail em log | `LogInformation`; evitar dados pessoais |
| 5 | `Program.cs` e deploy | Scalar e OpenAPI abertos em qualquer ambiente; o health check depende de `/scalar/v1` | restringir a Development e criar `/health` com checagem do banco |
| 6 | `AuthController.Logout` | exige access token válido (com token expirado não dá para revogar) e não confere o dono do refresh token | aceitar logout só com refresh token, ou validar a claim do usuário |
| 7 | `AuthController` | conflito detectado por `Contains("já cadastrado")`; `CreatedAtAction(nameof(Register))` aponta para um POST | códigos de erro tipados ou `ProblemDetails`; `Location` real |
| 8 | Pacotes | `10.0.0-preview.4` (JwtBearer, Logging e Configuration Abstractions) em projetos net10.0; Npgsql 9.0.4 na API e 9.0.3 no Admin | `Directory.Packages.props` e versões estáveis 10.0.x |
| 9 | `launchSettings.json` (API e Admin) | senha do PostgreSQL e `admin123` commitadas | user-secrets (a API já tem `UserSecretsId`) |
| 10 | `BasicAuthMiddleware` | `/_blazor` ignora a autenticação; comparação sem tempo constante | aceitável atrás do SSH tunnel; evoluir para autenticação por cookie e `[Authorize]` |
| 11 | `UsuariosSemCheckinAsync` | conta quem cadastrou ontem ou hoje como “em alerta” e usa `CURRENT_DATE` do banco (o DTO diz UTC) | ignorar cadastros recentes e definir o fuso (D1) |
| 12 | Android | `ApplicationId` do template (`com.companyname...`); `allowBackup="true"` (o backup automático pode copiar o SQLite com hash e tokens); manifest só com INTERNET e ACCESS\_NETWORK\_STATE | trocar o ID, desligar o backup e adicionar permissões nas Fases 3 e 6 |
| 13 | CI | `Mobile.App` nunca é compilado (o CI exclui MAUI) | job Android com `dotnet workload install maui-android` |

### 7.3 Prioridade baixa

- O deploy repete build e testes do CI e não depende do resultado dele.
- IP da VM e porta SSH aparecem em `docs/Ciclo-Desenvolvimento.md`; avaliar se o repositório é público.
- `FluentAssertions` 7.2.0 está adequado; as versões 8 em diante têm licença comercial. Manter a 7.x ou trocar de biblioteca antes de atualizar.
- `Result.MessageErro` é `string`, e o spec diz `string?`.

## 8. Decisões em aberto

**D1. Fuso horário das regras de 20h, 21h e 3º dia.**

- Contexto: `Checkin.Data` é um `DateOnly` enviado pelo aparelho, e os workers rodam no servidor. A documentação não diz em que fuso “20h” e “dia” são medidos.
- Opções: (a) fuso fixo `America/Sao_Paulo` em configuração; (b) fuso por usuário, em coluna própria (nome IANA).
- Recomendação: (a) no MVP. Um fuso único simplifica consultas, métricas e workers; registrar a decisão em `Arquitetura.md`.
- Evolução: adotar (b) quando houver usuários fora do Brasil, mantendo a data do check-in como a data local do aparelho.

**D2. Execução em segundo plano no Android (sync de 1h e heartbeat de 3h).**

- Contexto: a Arquitetura prevê `IHostedService`/`BackgroundService` no mobile, mas o .NET MAUI não traz o host genérico por padrão, e o Android limita trabalho em segundo plano (Doze e App Standby).
- Opções: WorkManager periódico (intervalo mínimo de 15 minutos, sem horário exato) em `Platforms/Android`, atrás de uma interface na Application; ou serviço em primeiro plano, com notificação permanente.
- Recomendação: WorkManager. Intervalos de 1h e 3h cabem na granularidade, e os avisos críticos partem do servidor por push, então não há necessidade de alarmes exatos locais.
- Evolução: se for exigida frequência maior ou mais garantia, avaliar o serviço em primeiro plano e as permissões exigidas nas versões novas do Android.

**D3. Onde guardar tokens e hash no mobile.**

- Contexto: a Arquitetura prevê Secure Storage; o design da Fase 2 prevê uma tabela no SQLite. O hash em SQLite comum é uma credencial reutilizável e entra no backup do Android.
- Recomendação: `SecureStorage` do MAUI (Keystore) para access token, refresh token e hash; SQLite só com `Usuario`. A regra “logado = existe `Usuario`” continua valendo, e a V003 deixa de existir.
- Evolução: exigir biometria para ler o hash no login silencioso.

## 9. Próximos passos sugeridos

1. Corrigir o item A e criar o teste de integração `register → login → logout`, antes do PR da Fase 1 para `master`.
2. Revisar o spec da Fase 2 (item D), decidir D3 e só então implementar as tasks 2–8.
3. Adicionar `GetByEmailAsync` e ajustar logs e mensagens de erro (item C e 7.2 #3 e #4).
4. Resolver a divergência da Task 9 e atualizar `Arquitetura.md` e `Ciclo-Desenvolvimento.md` (seção 6).
5. Trocar `INSERT OR REPLACE` por upsert com `ON CONFLICT` e criar os testes de regressão (7.2 #1) antes da Fase 3.
6. Criar `/health` e um job Android no CI.
7. Rascunhar os specs das Fases 3 e 4 já incorporando D1 e D2.
