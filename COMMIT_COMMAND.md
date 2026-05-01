# 🎯 COMANDO DE COMMIT - COPIE E COLE

## Opção 1: Usar arquivo de mensagem (RECOMENDADO)

```powershell
git add .
git commit -F COMMIT_MESSAGE_FINAL.md
git push origin master
```

## Opção 2: Mensagem direto no terminal

```powershell
git add .
git commit -m "feat(sprint-5): WebApp React + testes + deploy staging - 50% completa"
git push origin master
```

## Opção 3: Mensagem expandida (melhor visualização no GitHub)

```powershell
git add .
git commit -m "feat(sprint-5): WebApp React + testes + deploy staging - 50% completa" -m "
## Resumo
- WebApp React + TypeScript integrado com backend .NET 9
- Testes em 3 camadas: unitário (Jest/RTL), E2E (Cypress), integração (Testcontainers)
- Deploy Docker pronto para staging
- 11+ arquivos de documentação técnica

## Estatísticas
- 30 arquivos criados, ~2000+ linhas código
- 6 testes novos + 178 existentes
- Build: SUCCESS (0 erros)
- Sprint completude: 50% (5/10)
"
git push origin master
```

## Opção 4: Commit Interativo (Editor)

```powershell
git add .
git commit
# VS Code/Editor abre para escrever mensagem personalizada
git push origin master
```

---

## ✅ VERIFICAR ANTES DE FAZER COMMIT

```powershell
# Ver status
git status

# Ver o que será commitado
git diff --cached --stat

# Ver mudanças específicas
git diff --cached -- webapp/
git diff --cached -- docs/STATUS_PROJETO.md
```

---

## 📋 PRÓXIMOS PASSOS APÓS COMMIT

1. GitHub Actions roda automaticamente
2. Testes validam no CI/CD
3. Aguardar code review
4. Merge para master (após aprovação)
5. Deploy em staging

---

## ❓ SE ALGO DER ERRADO

### Desfazer alterações (não commitadas)
```powershell
git reset --hard HEAD
```

### Desfazer último commit (não pushado)
```powershell
git reset --soft HEAD~1
# Arquivos voltam para staged, pronto para novo commit
```

### Ver histórico de commits
```powershell
git log --oneline -10
```

---

## 🎉 SUCESSO!

Após rodar um dos comandos acima, seu commit estará:
- ✅ Feito localmente
- ✅ Pronto para push
- ✅ Com mensagem clara e descritiva
- ✅ Incluindo todas as alterações Sprint 5

Parabéns! 🚀
