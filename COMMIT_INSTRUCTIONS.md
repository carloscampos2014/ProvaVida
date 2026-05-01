# 📝 INSTRUÇÕES PARA COMMIT - SPRINT 5

## 1️⃣ Verificar Status do Git

```bash
git status
# Deve listar todos os arquivos novos em webapp/, testes e docs
```

## 2️⃣ Adicionar Alterações

```bash
# Adicionar todas as alterações
git add .

# Ou, se preferir, arquivo por arquivo:
git add webapp/
git add tests/ProvaVida.Infraestrutura.Tests/
git add docs/STATUS_PROJETO.md
git add SPRINT_5_RESUMO.md
git add SPRINT_5_COMMIT_ANALYSIS.md
```

## 3️⃣ Verificar o Diff (Opcional)

```bash
git diff --cached
# Revisa tudo que será commitado
```

## 4️⃣ Fazer o Commit

### Opção A: Mensagem Simples (Recomendado)
```bash
git commit -m "feat(sprint-5): WebApp React + testes + deploy staging

- Estrutura WebApp (React + TypeScript + Vite)
- Integração com backend .NET 9 (autenticação)
- Testes unitários, E2E e integração
- Base para deploy em staging (Docker)
- Documentação completa e atualizada"
```

### Opção B: Mensagem Detalhada (Usando arquivo)
```bash
git commit -F COMMIT_MESSAGE.md
```

### Opção C: Editor Interativo
```bash
git commit
# Abre editor (vim, nano, VSCode) para escrever mensagem
```

## 5️⃣ Verificar o Commit

```bash
git log -1 --stat
# Mostra o último commit e arquivos alterados

git log -1 -p
# Mostra o diff do último commit
```

## 6️⃣ Fazer Push (Se Necessário)

```bash
git push origin master
# Ou seu branch de desenvolvimento
```

## 📋 Checklist Antes de Commitar

- [ ] Build está rodando sem erros: `dotnet build`
- [ ] Testes backend passam: `dotnet test`
- [ ] WebApp foi criado em `webapp/`
- [ ] Documentação foi atualizada
- [ ] STATUS_PROJETO.md reflete o progresso
- [ ] Nenhum arquivo confidencial foi adicionado
- [ ] Mensagem de commit é clara e descritiva

## 🔍 Validar Após Commit

```bash
# Ver o commit criado
git show HEAD

# Ver último log
git log --oneline -5

# Ver branch status
git status
```

## 📖 Convenção de Commit (Usado)

```
type(scope): subject

body (opcional)

footer (opcional)
```

- **type:** `feat`, `fix`, `docs`, `test`, `chore`, `ci`
- **scope:** `sprint-5`, `webapp`, `backend`
- **subject:** Descrição curta (imperative mood)
- **body:** Detalhes das alterações
- **footer:** Breaking changes, issue refs

## ❌ Se Errou o Commit

### Adicionar arquivo esquecido
```bash
git add arquivo-esquecido
git commit --amend
# Abrirá o editor para editar mensagem
```

### Desfazer último commit (não pushado)
```bash
git reset --soft HEAD~1
# Arquivos voltam para staged, pronto para novo commit
```

### Desfazer tudo
```bash
git reset --hard HEAD~1
# ⚠️ Cuidado! Isso apaga o commit completamente
```

## 📝 Exemplo de Commit Feito

```
commit a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0
Author: Your Name <seu@email.com>
Date:   Wed Feb 5 10:30:00 2026 -0300

    feat(sprint-5): WebApp React + testes + deploy staging

    - Estrutura WebApp (React + TypeScript)
    - Integração com backend autenticação
    - Testes unitários, E2E, integração
    - Base para deploy em staging
    - Documentação atualizada
```

## 🎯 Resultado Final

Após commit com sucesso:
1. GitHub Actions deve rodar (se configurado)
2. Testes backend devem passar
3. Build deve ser bem-sucedido
4. Sprint 5 progride para ~50% de completude

---

**Próximo:** Esperar aprovação do PR (Pull Request) e fazer merge para `master`
