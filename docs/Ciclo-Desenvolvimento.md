# Ciclo de Desenvolvimento — ProvaVida

> Processo padrão para toda implementação do projeto. Toda issue segue este fluxo sem exceção.

---

## Visão Geral

```
Spec aprovado → Issues criadas → Branch → TDD → Commit → PR → Revisão → Merge
```

---

## 1. Planejamento (por fase)

```
1. Spec da fase criado (.kiro/specs/<fase>/)
      requirements.md → O que fazer
      design.md       → Como fazer
      tasks.md        → Lista atômica de tasks

2. Aprovação do spec pelo usuário

3. Issues criadas no GitHub — uma por task do tasks.md
      Label: fase-N-nome
      Título: [Fase N.X] Descrição da task

4. Branch de feature criada a partir de dev-refatoracao:
      feature/<descricao-da-task>
      Exemplo: feature/fase-0-criar-solucao-slnx
```

---

## 2. Implementação (por issue / TDD)

```
Red     → Escrever o teste que descreve o comportamento esperado (falha)
Green   → Implementar o mínimo para o teste passar
Refactor → Limpar o código sem quebrar o teste
```

### Commits

Formato: `tipo(escopo): #N descrição em imperativo`

```
feat(api.domain): #12 adicionar entidade Usuario
tests(api.domain): #12 adicionar testes para entidade Usuario
refactor(shared): #12 extrair IRepository para projeto Shared
```

- Um commit por mudança coesa — não aglomerar issues diferentes
- Build e testes devem passar antes de cada commit
- `Closes #N` no corpo do commit ou PR para fechar a issue automaticamente

---

## 3. Pull Request (feature → dev-refatoracao)

```
1. Push da branch feature para o remoto
      git push -u origin feature/<descricao>

2. Criar PR:
      Base: dev-refatoracao
      Head: feature/<descricao>
      Título: [Fase N.X] Descrição (máx 70 chars)
      Body: o que foi feito, o que foi testado, Closes #N

3. Revisão pelo usuário:
      Verificar Files Changed
      Aprovar ou solicitar mudanças

4. Merge após aprovação
      Método: Merge commit (preserva histórico)
```

---

## 4. Finalização de fase (dev-refatoracao → master)

```
1. Todas as issues da fase fechadas
2. Build e testes passando em dev-refatoracao
3. PR: dev-refatoracao → master
4. Revisão e merge
5. Próxima fase: novo spec → aprovação → issues
```

---

## Estrutura de Branches

| Branch | Propósito | Merge para |
|--------|-----------|-----------|
| `master` | Código estável, sempre buildando | — |
| `dev-refatoracao` | Integração das features | `master` |
| `feature/<descricao>` | Uma branch por issue | `dev-refatoracao` |

### Nomenclatura de branches

```
feature/fase-0-criar-solucao-slnx
feature/fase-0-configurar-dbup-api
feature/fase-1-endpoint-login
feature/fase-2-tela-checkin
```

---

## Ambientes de Debug

| Fase | Target | Como conectar |
|------|--------|---------------|
| Fases 0–5 | Windows Machine | F5 direto no Visual Studio |
| Fases 6–8 | Android 15 físico | WiFi via `adb pair` (sem USB) |

---

## Resumo do Ciclo em Uma Linha por Etapa

| Etapa | Ação |
|-------|------|
| Spec | Criar → aprovar |
| Issue | Criar no GitHub com label de fase |
| Branch | `feature/<descricao>` a partir de `dev-refatoracao` |
| Código | Red → Green → Refactor (TDD) |
| Commit | `tipo(escopo): #N descrição` |
| PR | `feature/` → `dev-refatoracao` — revisão obrigatória |
| Merge fase | `dev-refatoracao` → `master` |
