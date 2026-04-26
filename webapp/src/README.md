# Estrutura Inicial do ProvaVida WebApp

- `public/` — Arquivos estáticos (index.html)
- `src/app/` — Configuração global do app (rotas, providers)
- `src/components/` — Componentes reutilizáveis
- `src/features/` — Funcionalidades (ex: autenticação, check-in)
- `src/services/` — Serviços de API
- `src/hooks/` — React hooks customizados
- `src/types/` — Tipagens globais
- `src/tests/` — Testes unitários e mocks
- `src/main.tsx` — Entry point

## Como rodar

1. `npm install`
2. `npm start`

## Testes
- Unitários: `npm test`
- E2E: `npx cypress open`
