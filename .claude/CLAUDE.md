# ProvaVida

App mobile de check-in diario de bem-estar. Registra presenca uma vez por dia e notifica o contato de emergencia via e-mail, WhatsApp, SMS e voz apos 48h sem check-in.

Stack: .NET 10, ASP.NET Core, Dapper, PostgreSQL, Hangfire, .NET MAUI Android, Twilio, Plugin.LocalNotification

---

## Rules (carregadas automaticamente)

| Arquivo | Quando usar |
|---------|-------------|
| `rules/engineering-standards.md` | Toda implementação |
| `rules/workflow.md` | Branches, commits, PRs |
| `rules/01-result-pattern.md` | Padrão de retorno de erros |
| `rules/02-logging-observability.md` | Logging estruturado |
| `rules/03-testing-requirements.md` | Cobertura e padrões de teste |
| `rules/04-database-best-practices.md` | Banco e queries |

## Agents (subagentes especializados)

Invocar com `@<nome>` no chat:

| Agent | Quando invocar |
|-------|---------------|
| `@senior-developer` | Features, bugs, refactoring |
| `@solutions-architect` | Arquitetura, specs, ADRs |
| `@qa-engineer` | Planos de teste, cobertura, validação |
| `@business-analyst` | Requisitos, user stories, priorização |

## Skills (invocar com `/nome`)

| Skill | Comando |
|-------|---------|
| Code Review estruturado | `/code-review` |
| Criar spec completa | `/spec` |
| Debugar sistematicamente | `/debug` |
| Design arquitetural | `/arch` |

## Commands

| Command | O que faz |
|---------|-----------|
| `/generate-docs` | Gera/atualiza documentação técnica |
