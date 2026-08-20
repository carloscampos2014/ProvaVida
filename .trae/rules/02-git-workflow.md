# Regra: Workflow de Git

Estas regras se aplicam a toda operação de versionamento.

---

## Branches

- Sempre criar branch a partir de master atualizado: `feature/<descricao>`
- Padrão: `feature/fase-X-descricao` ou `feature/nome-da-feature`
- Nunca implementar diretamente em `main` ou `master`
- Publicar branch antes do primeiro commit de código

## Commits

Formato obrigatório (Conventional Commits):

```
tipo(escopo): #N descrição concisa
```

| Tipo | Quando |
|------|--------|
| `feat` | Nova funcionalidade |
| `fix` | Correção de bug |
| `docs` | Documentação |
| `refactor` | Refactoring sem mudança de comportamento |
| `tests` | Testes |
| `chore` | Configuração, infraestrutura |

- Máximo 72 caracteres na primeira linha
- Incluir `#N` quando avança ou fecha uma issue
- Build e testes devem passar antes de commitar

## Proibições absolutas

- `git push --force` ou `git push -f`
- `git reset --hard`
- `git clean -fd`
- `git push origin main` ou `git push origin master` diretamente
- `rm -rf` em diretórios de código-fonte
- Commitar `.env`, secrets ou arquivos de credenciais

## Pull Request

- Criar PR via `gh pr create` com `Closes #N` para fechar issues automaticamente
- Título máximo 70 caracteres
- Body com: resumo, mudanças por área, resultado dos testes
