# ProvaVida

App mobile de check-in diario de bem-estar. Registra presenca uma vez por dia e notifica o contato de emergencia via e-mail, WhatsApp, SMS e voz apos 48h sem check-in.

Stack: .NET 10, ASP.NET Core, Dapper, PostgreSQL, Hangfire, .NET MAUI Android, Twilio, Plugin.LocalNotification

---

## Instruções do projeto

Este arquivo é lido pelo Qwen Code antes de qualquer tarefa. O Qwen Code usa o
mesmo mecanismo de contexto do Gemini CLI — hierarquia de diretórios, arquivos
concatenados em ordem.

---

## Comandos do projeto

```bash
# Build
dotnet build src/ProvaVida.Api/ProvaVida.Api.csproj -c Debug --no-restore

# Testes
dotnet test tests/ProvaVida.Application.Tests/ --logger console;verbosity=minimal --no-restore

# Lint

```

---

## Padrões de engenharia

- SOLID obrigatório em toda implementação
- Nomes revelam intenção — sem variáveis genéricas (`data`, `temp`, `info`)
- Métodos com responsabilidade única e menos de 30 linhas
- Sem magic numbers — constantes nomeadas
- Sem deep nesting (máximo 3 níveis) — early returns e extração
- Sem secrets no código — variáveis de ambiente obrigatório
- Zero warnings no build
- Result Pattern para erros previsíveis — nunca `throw` para fluxo de negócio
- Testes unitários para toda nova lógica de negócio

---

## Arquitetura

Clean Architecture com separação clara de responsabilidades:

```
Domain        ← sem dependências externas
Application   ← use cases, depende de Domain
Infrastructure← implementações concretas (banco, email, HTTP)
Api           ← controllers finos, entrada/saída
```

---

## Workflow de Git

- Branches: `feature/<descricao>` a partir de master
- Commits: `tipo(escopo): #N descrição`
- Nunca push direto para `main` ou `master`
- Nunca `git push --force`, `git reset --hard`
- PR via `gh pr create` com `Closes #N`

---

## Comportamento esperado

- Ler o código existente antes de modificar — nunca inventar assinaturas
- Mostrar output real de build e testes antes de afirmar "feito"
- Features não triviais (3+ arquivos): briefing antes de implementar
- Ao corrigir bug: buscar mesmo padrão em todo o projeto (Twin Check)
- Seguir convenções do código existente — não introduzir novo padrão sem avisar
