# Cronograma de Desenvolvimento — ProvaVida

> Baseado no modelo SDD: cada fase produz um spec aprovado antes da implementação.
> Sequência pensada para construir as fundações primeiro e evitar retrabalho.

---

## Fase 0 — Fundação (Scaffolding)

**Objetivo:** Solução compilando, estrutura de projetos no lugar, pipelines base funcionando.

| # | Entrega | Descrição |
|---|---------|-----------|
| 0.1 | Solução `.slnx` | Criar `ProvaVida.slnx` com todos os projetos `.csproj` referenciados |
| 0.2 | Projetos base | Criar todos os projetos (Domain, Application, Infrastructure, Web/App) para Api, Mobile, Admin e Shared |
| 0.3 | Referências entre projetos | Configurar dependências entre camadas conforme Clean Architecture |
| 0.4 | Projetos de teste | Criar `ProvaVida.Api.Tests`, `ProvaVida.Mobile.Tests`, `ProvaVida.Admin.Tests` |
| 0.5 | DbUp — API | Configurar DbUp no `Api.Infrastructure`, criar migration inicial (tabelas `usuarios` e `checkins`) |
| 0.6 | DbUp — Mobile | Configurar DbUp no `Mobile.Infrastructure`, criar migration inicial SQLite |
| 0.7 | Repository base | Implementar `IRepository<T>` e `DapperRepository<T>` no Shared |
| 0.8 | DI base | Configurar injeção de dependência nos entry points (Api.Web, Mobile.App, Admin.Web) |

---

## Fase 1 — Autenticação (API)

**Objetivo:** API aceita login com hash, emite JWT, valida token, suporta refresh.

| # | Entrega | Descrição |
|---|---------|-----------|
| 1.1 | Entidade `Usuario` | Domain + repositório PostgreSQL |
| 1.2 | Cadastro | `POST /auth/register` — recebe hash, salva no banco |
| 1.3 | Login | `POST /auth/login` — compara hash, emite access token + refresh token |
| 1.4 | Refresh | `POST /auth/refresh` — renova access token |
| 1.5 | Logout | `POST /auth/logout` — invalida refresh token |
| 1.6 | Middleware JWT | Validação de token em rotas protegidas |
| 1.7 | Testes | Cobertura de todos os casos de autenticação |

---

## Fase 2 — Autenticação (Mobile)

**Objetivo:** App faz hash local, salva no SQLite, gerencia tokens silenciosamente.

| # | Entrega | Descrição |
|---|---------|-----------|
| 2.1 | Hash de senha | Utilitário SHA-256 no Mobile.Application |
| 2.2 | Entidade `Usuario` SQLite | Domain + repositório SQLite |
| 2.3 | Tela de Login | UI + ViewModel + use case de login |
| 2.4 | Tela de Cadastro | UI + ViewModel + use case de cadastro |
| 2.5 | Gerenciador de sessão | Lê SQLite para determinar estado de login |
| 2.6 | Gerenciador de token | Renovação silenciosa (refresh → login silencioso com hash) |
| 2.7 | Fluxo de inicialização | Lógica de abertura do app conforme `Fluxo-Inicialização.md` |
| 2.8 | Testes | Cobertura de autenticação e gerenciamento de sessão |

---

## Fase 3 — Check-in

**Objetivo:** Usuário consegue fazer check-in, dados persistidos local e sincronizados com servidor.

| # | Entrega | Descrição |
|---|---------|-----------|
| 3.1 | Entidade `Checkin` | Domain + repositórios (PostgreSQL e SQLite) |
| 3.2 | Endpoint check-in API | `POST /checkins` — valida um por dia, persiste |
| 3.3 | Tela de Check-in | UI + ViewModel + use case |
| 3.4 | Check-in offline | Salva local com `Sincronizado = false` quando sem internet |
| 3.5 | Validação duplicata | Impede mais de um check-in por dia |
| 3.6 | Testes | Cobertura dos fluxos online e offline |

---

## Fase 4 — Sincronismo e Heartbeat

**Objetivo:** Dados fluem entre device e servidor automaticamente.

| # | Entrega | Descrição |
|---|---------|-----------|
| 4.1 | Endpoint de sincronismo API | `GET /sync` e `POST /sync` — retorna delta e aceita pendentes |
| 4.2 | SyncWorker (Mobile) | Background service a cada 1h — pull + push |
| 4.3 | HeartbeatWorker (Mobile) | Background service a cada 3h — `POST /heartbeat` |
| 4.4 | Endpoint heartbeat API | `POST /heartbeat` — registra sinal de vida |
| 4.5 | Exclusão por sincronismo | Detecta conta deletada no servidor, limpa SQLite, redireciona para Login |
| 4.6 | Testes | Cobertura de sincronismo, heartbeat e casos de borda |

---

## Fase 5 — Gestão de Conta (Mobile)

**Objetivo:** Usuário consegue alterar e excluir sua conta.

| # | Entrega | Descrição |
|---|---------|-----------|
| 5.1 | Alterar conta API | `PUT /account` — atualiza dados e hash |
| 5.2 | Excluir conta API | `DELETE /account` — remove dados do servidor |
| 5.3 | Tela Alterar Conta | UI + ViewModel + use case |
| 5.4 | Tela Excluir Conta | UI + ViewModel + use case + confirmação |
| 5.5 | Logoff | Limpa SQLite, redireciona para Login |
| 5.6 | Testes | Cobertura de alteração, exclusão e logoff |

---

## Fase 6 — Notificações Push

**Objetivo:** Servidor envia push notifications nos momentos certos.

| # | Entrega | Descrição |
|---|---------|-----------|
| 6.1 | Integração FCM/APNs | Configurar push notification no projeto Api |
| 6.2 | Registro de device token | App envia device token ao servidor no login |
| 6.3 | NotificacaoPushUsuarioWorker | Worker agendado 20h — push para usuários sem check-in no dia |
| 6.4 | NotificacaoPushEmergenciaWorker | Worker agendado 21h D+2 — push para usuários 2 dias sem check-in |
| 6.5 | Testes | Cobertura dos workers e lógica de elegibilidade |

---

## Fase 7 — Notificações Multicanal (Contato de Emergência)

**Objetivo:** No 3º dia sem check-in, acionar contato de emergência por múltiplos canais.

| # | Entrega | Descrição |
|---|---------|-----------|
| 7.1 | Integração e-mail | Configurar serviço de e-mail (SMTP ou provider) |
| 7.2 | Integração WhatsApp | Configurar API WhatsApp Business |
| 7.3 | Integração SMS | Configurar provider de SMS |
| 7.4 | Integração ligação de voz | Configurar provider de voz (ex: Twilio) |
| 7.5 | NotificacaoMulticanalEmergenciaWorker | Worker D+3 — dispara e-mail + WhatsApp + SMS em paralelo; fallback ligação |
| 7.6 | Registro de notificações | Persistir histórico de notificações enviadas |
| 7.7 | Testes | Cobertura do worker e lógica de fallback |

---

## Fase 8 — Painel Administrativo

**Objetivo:** Painel Blazor funcional para monitoramento e operação.

| # | Entrega | Descrição |
|---|---------|-----------|
| 8.1 | Scaffolding Admin | Projeto Blazor Server, Basic Auth, conexão PostgreSQL direta |
| 8.2 | Dashboard usuários | Lista de cadastrados e com check-in atrasado |
| 8.3 | Dashboard notificações | Histórico de notificações enviadas (usuário e emergência) |
| 8.4 | Dashboard check-ins | Histórico de check-ins por usuário |
| 8.5 | Testes de envio | Disparo manual de e-mail, WhatsApp, SMS e ligação |
| 8.6 | Controle de backup | Interface para backup do banco PostgreSQL |
| 8.7 | Testes | Cobertura das queries e lógicas do painel |

---

## Resumo por fase

| Fase | Nome | Pré-requisito |
|------|------|---------------|
| 0 | Scaffolding | — |
| 1 | Auth API | Fase 0 |
| 2 | Auth Mobile | Fase 1 |
| 3 | Check-in | Fase 2 |
| 4 | Sincronismo e Heartbeat | Fase 3 |
| 5 | Gestão de Conta | Fase 4 |
| 6 | Notificações Push | Fase 5 |
| 7 | Notificações Multicanal | Fase 6 |
| 8 | Painel Admin | Fase 7 |

---

> Cada fase inicia com a criação do spec (`.kiro/specs/<fase>/`) e só é implementada após aprovação.
