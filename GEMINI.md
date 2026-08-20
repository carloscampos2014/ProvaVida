# ProvaVida

App mobile de check-in diario de bem-estar. Registra presenca uma vez por dia e notifica o contato de emergencia via e-mail, WhatsApp, SMS e voz apos 48h sem check-in.

Stack: .NET 10, ASP.NET Core, Dapper, PostgreSQL, Hangfire, .NET MAUI Android, Twilio, Plugin.LocalNotification

---

## Instruções gerais

Este arquivo é carregado pelo Gemini CLI em toda sessão. Define as convenções,
comandos e expectativas do projeto.

Use `@./caminho/arquivo.md` para importar regras específicas de subcomponentes.

---

## Comandos do projeto

```bash
# Build
dotnet build src/ProvaVida.Api/ProvaVida.Api.csproj -c Debug --no-restore

# Testes
dotnet test tests/ProvaVida.Application.Tests/ --logger console;verbosity=minimal --no-restore

# Lint

```

Sempre verificar build e testes após qualquer alteração de código.

---

## Padrões de código

### Nomenclatura
- Classes: PascalCase
- Métodos/funções: PascalCase (C#) / camelCase (TS/JS/Python)
- Variáveis: camelCase / snake_case
- Constantes: UPPER_SNAKE_CASE ou readonly PascalCase
- Interfaces C#: prefixo `I`
- Arquivos: PascalCase.cs / kebab-case.ts / snake_case.py

### Regras obrigatórias
- SOLID em toda implementação
- Métodos com responsabilidade única, máximo 30 linhas
- Sem magic numbers — constantes nomeadas
- Sem deep nesting (máximo 3 níveis) — usar early returns
- Sem secrets inline — variáveis de ambiente sempre
- Zero warnings no build
- Result Pattern para erros previsíveis de negócio

---

## Arquitetura

```
Domain        ← sem dependências externas
    ↑
Application   ← interfaces aqui, depende apenas de Domain
    ↑
Infrastructure← implementa interfaces de Application
    ↑
Api           ← controllers finos, delega para Application
```

---

## Git e workflow

- Branches: `feature/<descricao>` a partir de master atualizado
- Commits: `tipo(escopo): #N descrição` (Conventional Commits)
- Nunca push direto para main/master
- Nunca `git push --force`, `git reset --hard`, `rm -rf`
- Criar PR via `gh pr create` com `Closes #N`

---

## Comportamento esperado

- Ler arquivos relevantes antes de modificar — nunca inventar assinaturas
- Mostrar output real de build/testes — nunca afirmar "concluído" sem evidência
- Para features com 3+ arquivos: fazer briefing e aguardar aprovação
- Ao corrigir bug: buscar o mesmo padrão no projeto inteiro (Twin Check)
- Seguir padrão do código existente — não introduzir novo estilo sem avisar

---

## Importações de contexto específico

```
@./docs/ARCHITECTURE.md
```

Use `@caminho` para adicionar contexto de subcomponentes quando relevante.


---

# Padrão de Briefing Detalhado

Todo briefing de feature ou correção de bug deve incluir:

## Para cada arquivo alterado

- **O que está errado hoje** — comportamento atual com exemplo concreto de código
- **Por que é um problema** — consequência real para o usuário ou sistema
- **O que exatamente vai mudar** — trecho de código antes vs. depois (mesmo que resumido)
- **Por que essa abordagem** — decisão de design em 1 frase

## Formato obrigatório por item

```
**Arquivo: NomeDoArquivo.cs**
Problema: [o que acontece hoje, com código ou fluxo concreto]
Consequência: [o que o usuário ou sistema experimenta]
Correção: [o que muda — antes/depois ou descrição precisa]
Decisão: [por que essa abordagem e não outra]
```

## Nível de detalhe esperado

- Nomear métodos específicos que serão alterados
- Mostrar a assinatura antes e depois quando mudar
- Explicar o mecanismo do bug (race condition, null ref, state compartilhado, etc.)
- Explicar o mecanismo da correção (semáforo, header por request, OnAppearing, etc.)

## O que NÃO fazer

- Listar arquivos sem explicar o que muda em cada um
- Usar termos vagos como "melhora a robustez" sem explicar como
- Omitir o "por quê" — toda decisão precisa de razão
