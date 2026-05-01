feat(sprint-5): WebApp React + testes + base para deploy em staging

## 📋 Alterações Principais

### WebApp - Estrutura e Integração
- Criação do projeto WebApp com React 18 + TypeScript 5
- Integração com backend .NET 9 (serviço de autenticação)
- Componentes reutilizáveis (Button, LoginForm)
- Cliente HTTP Axios configurado

### Testes - Frontend
- Testes unitários: Button, authService, LoginForm (Jest + RTL)
- Testes E2E: cenários de login com Cypress
- Cobertura básica pronta para expansão

### Testes - Backend (Integração)
- TestcontainersPostgresFixture para testes opcionais
- Priorização de SQLite para CI/CD (GitHub Actions)
- Documentação de testes de integração
- Trait [Opcional-Postgres] para testes condicionais

### Deploy - Staging
- Dockerfile multi-stage para WebApp
- docker-compose.staging.yml (orquestração webapp + backend)
- Script deploy-staging.sh (automação)
- .env.example (configuração)
- DEPLOY.md (documentação)

### Documentação
- webapp/README.md - instruções do projeto
- webapp/DEPLOY.md - guia de deploy
- webapp/cypress/README.md - testes E2E
- tests/ProvaVida.Infraestrutura.Tests/README.md - testes integração
- SPRINT_5_RESUMO.md - resumo completo das alterações

## 📊 Estatísticas

- **Arquivos Criados:** ~30 novos arquivos
- **Linhas de Código:** ~2000+ (frontend + config)
- **Testes Criados:** 5 (3 unitários + 2 E2E)
- **Build Status:** ✅ 0 erros
- **Sprint 5 Status:** 50% completa (5/10 testes)

## 🚀 Como Testar

### WebApp
```bash
cd webapp
npm install && npm start
# Testes: npm test (unitários) ou npx cypress open (E2E)
```

### Deploy Staging
```bash
cd webapp && bash scripts/deploy-staging.sh
# Acessar: http://localhost:5173
```

### Testes Backend
```bash
dotnet test
```

## 🔗 Referências

- Documentação: /docs/STATUS_PROJETO.md
- Detalhes Técnicos: SPRINT_5_RESUMO.md
- Deploy: webapp/DEPLOY.md

## ⚠️ Notas Importantes

- SQLite é prioridade para CI/CD (GitHub Actions)
- PostgreSQL com Testcontainers é opcional e marcado com [Trait]
- Backend deve estar rodando em localhost:5000 para testes completos
- Docker é obrigatório para deploy em staging

---

**Tipo:** feature
**Escopo:** Sprint 5
**Relacionado:** #sprint-5, #webapp, #testes, #deploy
