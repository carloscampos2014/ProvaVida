# ProvaVida - Sistema de Monitoramento de Segurança Pessoal

[![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12-239120?logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Clean Architecture](https://img.shields.io/badge/Architecture-Clean-success)](docs/ARQUITETURA.md)
[![SOLID Principles](https://img.shields.io/badge/SOLID-Compliant-success)](docs/DIRETRIZES_IA.md)
[![xUnit Tests](https://img.shields.io/badge/Tests-xUnit%202.6.0-00C8FF?logo=unittest)](https://xunit.net/)
[![Test Coverage](https://img.shields.io/badge/Coverage-75%25%2B-brightgreen)](docs/BACKLOG_AGILE.md)
[![EF Core](https://img.shields.io/badge/ORM-EF%20Core%209.0-512BD4)](https://docs.microsoft.com/en-us/ef/core/)
[![PostgreSQL](https://img.shields.io/badge/DB-PostgreSQL-336791?logo=postgresql)](https://www.postgresql.org/)
[![SQLite](https://img.shields.io/badge/DB-SQLite-003B57?logo=sqlite)](https://www.sqlite.org/)

---

## 🎯 Status do Projeto

### 📊 Progresso Geral
```
Sprint 1 ✅ CONCLUÍDA    [████████████████████] 100% - Domínio Puro
Sprint 2 ✅ CONCLUÍDA    [████████████████████] 100% - Infraestrutura
Sprint 3 ⏳ PRÓXIMA      [████████░░░░░░░░░░░░]  40% - Check-in e Histórico
Sprint 4 📅 PLANEJADA    [░░░░░░░░░░░░░░░░░░░░]   0% - Sistema de Alertas
Sprint 5 📅 PLANEJADA    [░░░░░░░░░░░░░░░░░░░░]   0% - API REST
Sprint 6 📅 PLANEJADA    [░░░░░░░░░░░░░░░░░░░░]   0% - QA + Deploy
```

### 📈 Métricas Atuais
- **Testes Total:** 75/75 ✅ PASSANDO
  - Domínio: 60/60 ✅
  - Infraestrutura: 15/15 ✅
- **Cobertura:** 75%+ 
- **Build Status:** ✅ Sucesso (0 erros, 0 avisos)
- **Camadas Implementadas:** 2/4
  - ✅ Domínio (Completo)
  - ✅ Infraestrutura (Completo)
  - ⏳ Aplicação (Sprint 3)
  - ⏳ API (Sprint 5)

### 🚀 Próximas Prioridades (Sprint 3)
- [ ] ServicoCheckIn com lógica de 48h
- [ ] Histórico FIFO (máx 5 registros)
- [ ] ServicoNotificacao (limpeza de alertas)
- [ ] DTOs para CheckIn e Notificação
- [ ] Testes de integração

---

## 📚 Documentação Completa

### 📖 Documentação de Negócio
* [**Especificações de Negócio**](docs/ESPECIFICACOES.md) - Regras de 48h, prazos e protocolos de emergência
* [**User Stories & Backlog Agile**](docs/USER_STORIES.md) - Histórias de usuário detalhadas
* [**Backlog Agile**](docs/BACKLOG_AGILE.md) - Planejamento por sprint com estimativas

### 🏗️ Documentação Técnica
* [**Arquitetura do Sistema**](docs/ARQUITETURA.md) - Clean Architecture e estrutura de camadas
* [**Modelagem de Dados**](docs/MODELAGEM.md) - Entidades, properties e relacionamentos
* [**Diagrama Arquitetura BD**](docs/DIAGRAMA_ARQUITETURA_BD.md) - Visualização do banco de dados
* [**Suporte Múltiplos Bancos**](docs/SUPORTE_MULTIPLOS_BANCOS.md) - Factory Pattern para SQLite/PostgreSQL/SQL Server
* [**Arquitetura de Alertas**](docs/ARQUITETURA_ALERTAS.md) - Serviço de alertas 24/7 desacoplado

### 👥 Documentação de Padrões
* [**Diretrizes para IA**](docs/DIRETRIZES_IA.md) - Padrões de código, SOLID e Clean Code (Português)
* [**Papéis de IA**](docs/PAPEIS_IA.md) - Definição de papéis (PO, Analista, Arquiteto, Dev, QA)

---

## 🛠 Tecnologias & Stack

### Backend
| Tecnologia | Versão | Uso |
|-----------|--------|-----|
| **.NET** | 9.0 | Framework principal |
| **C#** | 12 | Linguagem |
| **Entity Framework Core** | 9.0 | ORM |
| **xUnit** | 2.6.0 | Testes unitários |
| **BCrypt.Net-Next** | 4.0.3 | Hash de senha |
| **Quartz.NET** | 3.8+ | Job scheduler (Sprint 4) |
| **SignalR** | 9.0 | Notificações real-time (Sprint 5) |

### Banco de Dados
- **SQLite** 9.0.0 - Desenvolvimento local
- **PostgreSQL** 9.0.1 - Produção (Linux/Mac)
- **SQL Server** - Suporte futuro (Infrastructure as Code)

### Arquitetura
- **Clean Architecture** - Separação de responsabilidades
- **SOLID Principles** - Código manutenível e testável
- **Injeção de Dependência** - Built-in .NET
- **Factory Pattern** - Múltiplos provedores de BD
- **Result Pattern** - Tratamento de erros elegante

### Frontend (Planejado)
- **Sprint 5:** Webapp (React/Vue + TypeScript)
- **Sprint 7+:** MAUI (iOS/Android)
- **Sprint 9+:** WPF/WinUI Windows (opcional)

---

## 📋 Estrutura do Projeto

```
ProvaVida/
├── src/
│   ├── ProvaVida.Dominio/              # ✅ Camada de Domínio (Completo)
│   │   ├── Entidades/
│   │   ├── ObjetosValor/
│   │   ├── Enums/
│   │   ├── Repositorios/ (interfaces)
│   │   └── Exceções/
│   │
│   ├── ProvaVida.Infraestrutura/       # ✅ Camada de Infraestrutura (Completo)
│   │   ├── Contexto/
│   │   ├── Repositorios/ (implementação)
│   │   ├── Servicos/
│   │   ├── Mappings/
│   │   └── Configuracao/
│   │
│   ├── ProvaVida.Aplicacao/            # ⏳ Em progresso (Sprint 3)
│   │   ├── Servicos/
│   │   ├── DTOs/
│   │   └── Validadores/
│   │
│   └── ProvaVida.API/                  # 📅 Planejado (Sprint 5)
│       ├── Controllers/
│       ├── Middleware/
│       └── Hubs/ (SignalR)
│
├── tests/
│   ├── ProvaVida.Dominio.Tests/        # ✅ 60/60 testes passando
│   └── ProvaVida.Infraestrutura.Tests/ # ✅ 15/15 testes passando
│
├── docs/
│   ├── ARQUITETURA.md
│   ├── ARQUITETURA_ALERTAS.md
│   ├── BACKLOG_AGILE.md
│   ├── USER_STORIES.md
│   ├── MODELAGEM.md
│   ├── ESPECIFICACOES.md
│   ├── DIRETRIZES_IA.md
│   ├── PAPEIS_IA.md
│   ├── DIAGRAMA_ARQUITETURA_BD.md
│   ├── SUPORTE_MULTIPLOS_BANCOS.md
│   └── RESPOSTA_MULTIPLOS_BANCOS.md
│
└── ProvaVida.sln
```

---

## ✅ Funcionalidades Implementadas

### Sprint 1 - ✅ Domínio Puro
- [x] Entidade Usuario com factory methods
- [x] Entidade CheckIn com cálculo de 48h
- [x] Entidade ContatoEmergencia com validações
- [x] Entidade Notificacao com tipos
- [x] Enums (StatusCheckIn, TipoNotificacao, MeioNotificacao, StatusNotificacao, StatusUsuario)
- [x] Value Objects (Email, Telefone)
- [x] Exceções personalizadas de domínio
- [x] 60 testes unitários ✅

### Sprint 2 - ✅ Infraestrutura
- [x] DbContext com EF Core 9.0
- [x] Repositório genérico (CRUD base)
- [x] 4 Repositórios específicos (Usuario, CheckIn, ContatoEmergencia, Notificacao)
- [x] Serviço de Hash com BCrypt (12 rounds)
- [x] Injeção de Dependência configurada
- [x] 4 Mappings EF Core
- [x] Factory Pattern para múltiplos bancos
- [x] Suporte SQLite (dev) + PostgreSQL (prod)
- [x] 15 testes de integração ✅

### Sprint 3 - ⏳ Em Progresso
- [ ] ServicoCheckIn (registrar + resetar 48h)
- [ ] Lógica de Histórico FIFO
- [ ] ServicoNotificacao (limpeza)
- [ ] DTOs para requisições
- [ ] Testes unitários para serviços

---

## 🚀 Como Executar

### Pré-requisitos
- .NET 9 SDK
- Visual Studio 2022 / VS Code
- Git

### Passos
```bash
# 1. Clonar repositório
git clone https://github.com/seu-usuario/ProvaVida.git
cd ProvaVida

# 2. Restaurar dependências
dotnet restore

# 3. Compilar
dotnet build

# 4. Executar testes
dotnet test

# 5. Executar aplicação (Sprint 5+)
dotnet run --project src/ProvaVida.API/ProvaVida.API.csproj
```

---

## 📊 Métricas de Qualidade

| Métrica | Meta | Atual | Status |
|---------|------|-------|--------|
| Cobertura de Testes | > 75% | 75%+ | ✅ |
| Testes Passando | 100% | 75/75 | ✅ |
| Build | 0 erros | 0 erros | ✅ |
| SOLID Compliance | Sim | Sim | ✅ |
| Documentação | Completa | 80% | 🟡 |

---

## 🤝 Contribuições

Este projeto foi desenvolvido com foco em **Clean Architecture** e **Test-Driven Development (TDD)** para garantir:
- ✅ Código testável e manutenível
- ✅ Baixo acoplamento entre camadas
- ✅ Fácil extensibilidade (novos provedores de BD, serviços)
- ✅ Documentação técnica completa

Seguimos **SOLID Principles** e padrões da comunidade .NET.

---

## 📞 Suporte

Para dúvidas técnicas, consulte:
- [📖 Documentação Completa](docs/)
- [🏗️ Arquitetura](docs/ARQUITETURA.md)
- [📋 Backlog](docs/BACKLOG_AGILE.md)

---

*Este projeto foi desenvolvido com auxiliado por Agentes de IA (GitHub Copilot) e documentado em Português (BR) para máxima clareza.*

**Última atualização:** 31 de janeiro de 2026  
**Versão:** 1.0-MVP  
**Ciclo:** 48 horas por Sprint
