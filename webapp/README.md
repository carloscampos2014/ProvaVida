# ProvaVida WebApp

Frontend profissional do sistema ProvaVida, desenvolvido em **React 18** + **TypeScript** + **Vite**.

## 🚀 Stack Técnico

| Tecnologia | Versão | Propósito |
|-----------|--------|----------|
| React | 18.2+ | UI Framework |
| TypeScript | 5.3+ | Type Safety |
| Vite | 8.0+ | Build Tool |
| Axios | 1.6+ | HTTP Client |
| Jest | 29.7+ | Testes Unitários |
| React Testing Library | 14.0+ | Component Testing |
| Cypress | 14.2+ | Testes E2E |
| CSS Modules | - | Styling |

## 📂 Estrutura de Pastas

```
webapp/
├── src/
│   ├── components/              # ✅ Componentes Reutilizáveis
│   │   ├── Button/
│   │   ├── StatusCheckIn/
│   │   ├── BotaoCheckIn/
│   │   ├── ListaContatos/
│   │   ├── HistoricoCheckIns/
│   │   ├── Notificacoes/
│   │   └── index.ts            # Barrel exports
│   │
│   ├── features/               # ✅ Features Específicas
│   │   ├── auth/               # Componentes de autenticação
│   │   │   ├── LoginForm.tsx
│   │   │   ├── CadastroForm.tsx
│   │   │   └── authService.ts
│   │   │
│   │   └── dashboard/          # Container do dashboard
│   │       ├── Dashboard.tsx
│   │       ├── Dashboard.module.css
│   │       ├── dashboardService.ts
│   │       └── Dashboard.module.css
│   │
│   ├── services/               # ✅ Serviços Genéricos
│   │   ├── api.ts             # Axios client
│   │   └── dashboardService.ts # Integração com API
│   │
│   ├── main.tsx               # Entry point (React 18 + TypeScript)
│   └── setupTests.ts          # Configuração Jest
│
├── cypress/
│   └── e2e/
│       ├── inicial.cy.ts
│       ├── login.cy.ts
│       └── dashboard.cy.ts    # ✅ Novo - Testes do dashboard
│
├── tests/                      # ✅ Testes Unitários
│   └── ProvaVida.WebApp.Tests/
│       └── features/dashboard/
│           ├── Dashboard.test.tsx
│           └── dashboardService.test.ts
│
├── package.json
├── tsconfig.json
├── vite.config.ts
└── README.md
```

## ✨ Componentes Criados (Sprint 5)

### 1️⃣ StatusCheckIn
Exibe o status do próximo check-in com:
- ⏱️ Tempo restante em formato legível
- 🎨 Cores dinâmicas (OK → Atenção → Crítico → Vencido)
- 📅 Data do próximo vencimento
- 🔄 Atualização automática a cada minuto

```typescript
import { StatusCheckIn } from "../../components";
<StatusCheckIn status={dados.statusCheckIn} />
```

### 2️⃣ BotaoCheckIn
Botão interativo para realizar check-in:
- ⏳ Estado de carregamento com spinner
- ⚠️ Variação crítica quando vencido
- 📱 Responsivo e acessível

```typescript
import { BotaoCheckIn } from "../../components";
<BotaoCheckIn 
  onClick={handleCheckIn}
  carregando={fazendoCheckIn}
  vencido={status.vencido}
/>
```

### 3️⃣ ListaContatos
Lista de contatos de emergência:
- 👤 Nome, email e WhatsApp
- 🔴 Badge de inativo se não ativo
- 🎯 Hover effects

```typescript
import { ListaContatos } from "../../components";
<ListaContatos contatos={dados.contatos} />
```

### 4️⃣ HistoricoCheckIns
Tabela responsiva com últimos 5 check-ins:
- 📊 Data do check-in, próximo vencimento, notificações
- 📱 Responsiva em todas as telas

```typescript
import { HistoricoCheckIns } from "../../components";
<HistoricoCheckIns historico={dados.historico} />
```

### 5️⃣ Notificacoes
Badge pulsante de notificações pendentes:
- 🔔 Ícone com animação
- 🎯 Conta até 9+ notificações

```typescript
import { Notificacoes } from "../../components";
<Notificacoes quantidade={dados.notificacoesPendentes} />
```

## 🔗 Serviços de API

### authService
Integração com endpoints de autenticação:
```typescript
await authService.loginUsuario({ email, senha });
await authService.cadastrarUsuario(dados, contato);
```

### dashboardService
Integração com endpoints do dashboard:
```typescript
await dashboardService.obterDadosDashboard();
await dashboardService.fazerCheckIn();
await dashboardService.obterStatusCheckIn();
await dashboardService.obterContatos();
await dashboardService.obterHistoricoCheckIns();
await dashboardService.obterNotificacoesPendentes();
```

## 🧪 Testes

### Jest + React Testing Library
Testes unitários de componentes:
```bash
npm run test
```

**Cobertura:**
- ✅ 10 testes do Dashboard
- ✅ 11 testes do dashboardService
- ✅ 8 testes E2E (Cypress)

### Cypress E2E
Testes de fluxo completo:
```bash
npm run cypress
```

**Cenários:**
- ✅ Dashboard carrega com dados
- ✅ Status do check-in exibe corretamente
- ✅ Botão check-in funciona
- ✅ Contatos exibem
- ✅ Histórico exibe
- ✅ Logout funciona
- ✅ Responsividade (mobile)
- ✅ Tratamento de erros

## 🚀 Como Iniciar

### Pré-requisitos
```bash
Node.js 18+
npm ou yarn
```

### Instalação
```bash
cd webapp
npm install
```

### Desenvolvimento
```bash
npm run start
```
Abrirá em `http://localhost:5173`

### Build para Produção
```bash
npm run build
```

### Testes
```bash
# Unitários
npm run test

# E2E
npm run cypress
```

## 🎨 CSS Modular

Cada componente possui seu próprio arquivo `.module.css`:
```typescript
import styles from "./Component.module.css";
<div className={styles.container}>
```

**Benefícios:**
- ✅ Sem conflito de classes
- ✅ Fácil manutenção
- ✅ Escalável
- ✅ Type-safe

## 🔐 Segurança

- ✅ Tokens salvos em localStorage
- ✅ API client com Axios
- ✅ Validação de formulários
- ✅ Error handling robusto
- ✅ Testes de segurança

## 📱 Responsividade

- ✅ Mobile (< 768px)
- ✅ Tablet (768px - 1024px)
- ✅ Desktop (> 1024px)

## 🤝 Integração com Backend

API conectada em: `http://localhost:5000` (dev)

Endpoints utilizados:
- `POST /api/auth/login`
- `POST /api/auth/registrar`
- `GET /api/dashboard`
- `POST /api/checkin`
- `GET /api/checkin/status`
- `GET /api/checkin/historico`
- `GET /api/contatos-emergencia`
- `GET /api/notificacoes/pendentes`

## 🛠 Deploy

Veja [DEPLOY.md](./DEPLOY.md) para instruções de deploy em staging e produção.

## 📚 Documentação Adicional

- [USER_STORIES.md](../docs/USER_STORIES.md) - Casos de uso
- [ESPECIFICACOES.md](../docs/ESPECIFICACOES.md) - Requisitos
- [ARQUITETURA.md](../docs/ARQUITETURA.md) - Design de arquitetura

## 📝 Convenções de Código

- ✅ PascalCase para componentes e classes
- ✅ camelCase para funções e variáveis
- ✅ UPPER_CASE para constantes
- ✅ Português em comentários e strings
- ✅ TypeScript strict mode ativado
- ✅ ESLint + Prettier configurados

## 📋 Roadmap Sprint 5 Restante (30%)

- [ ] Deploy em staging
- [ ] Testes E2E em staging
- [ ] Validação de funcionalidades críticas
- [ ] Otimização de performance
