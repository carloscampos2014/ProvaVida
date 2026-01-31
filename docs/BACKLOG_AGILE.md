# Backlog Agile - ProvaVida

**Última Atualização:** 31 de janeiro de 2026  
**Versão:** 1.0 - MVP  
**Ciclo de Desenvolvimento:** 48 horas por Sprint

---

## 🎯 Visão Geral do Produto

O **ProvaVida** é um sistema de monitoramento de segurança pessoal que:
- Implementa ciclos de **48 horas** para prova de vida (check-in)
- Gera **alertas progressivos** (lembretes em -6h e -2h)
- Dispara **notificações de emergência** aos contatos após vencimento
- Mantém **histórico reduzido** (máx. 5 registros por tipo)
- Requer **mínimo 1 contato de emergência** para ativar monitoramento

---

## 📊 Épicos

| Épico | Descrição | Sprints |
|-------|-----------|---------|
| **E01** | Fundação do Domínio (Puro) | S1 |
| **E02** | Infraestrutura e Repositórios | S2 |
| **E03** | Gestão de Usuários e Contatos | S2-S3 |
| **E04** | Sistema de Check-in | S3-S4 |
| **E05** | Sistema de Alertas | S4-S5 |
| **E06** | API REST e Endpoints | S5 |
| **E07** | Testes e Qualidade | S6 |

---

## 📅 Sprint 1 - Domínio Puro e Independente

**Duração:** 48 horas  
**Objetivo:** Estabelecer o núcleo da lógica de negócio (Entidades e Enums) sem dependências externas  
**Status:** 🔵 Não Iniciada  
**Princípio:** Domínio totalmente independente de Infraestrutura (EF Core, Banco, etc.)

### Funcionalidades

| ID | Funcionalidade | Prioridade | Estimativa | AC |
|---|---|---|---|---|
| **F01.01** | Criar entidades de Domínio (`Usuario`, `ContatoEmergencia`, `CheckIn`, `Notificacao`) | 🔴 CRÍTICA | 4h | ✅ Entidades compilam, sem EntityFramework |
| **F01.02** | Definir Enums (`StatusCheckIn`, `TipoNotificacao`, `MeioNotificacao`, `StatusNotificacao`) | 🔴 CRÍTICA | 2h | ✅ Enums utilizáveis nas entidades |
| **F01.03** | Criar interfaces de Repositório no Domínio (apenas contrato) | 🔴 CRÍTICA | 2h | ✅ Interfaces documentadas, sem implementação |
| **F01.04** | Implementar regras de negócio em entidades (cálculo 48h, validações) | 🔴 CRÍTICA | 3h | ✅ Métodos ValidarCheckIn(), CalcularVencimento(), etc. |
| **F01.05** | Criar exceções de Domínio personalizadas | 🔴 CRÍTICA | 2h | ✅ UsuarioInvalidoException, ContatoObrigatorioException, etc. |
| **F01.06** | Testes unitários para entidades (xUnit) | 🟠 ALTA | 4h | ✅ Cobertura > 85% do Domínio |
| **F01.07** | Documentar Value Objects e Invariantes de Negócio | 🟠 ALTA | 1h | ✅ Readme técnico do Domínio |

**Total Sprint 1:** 18 horas

---

## 📅 Sprint 2 - Infraestrutura e Serviços de Aplicação

**Duração:** 48 horas  
**Objetivo:** Implementar persistência (Repositórios) e orquestração de casos de uso  
**Status:** 🔵 Não Iniciada  
**Princípio:** Infraestrutura implementa contratos do Domínio; Aplicação orquestra sem lógica de negócio

### Funcionalidades

| ID | Funcionalidade | Prioridade | Estimativa | AC |
|---|---|---|---|---|
| **F02.01** | Configurar DbContext e Migrations (EF Core) | 🔴 CRÍTICA | 3h | ✅ Banco criado, migrations testadas |
| **F02.02** | Implementar Repositórios genéricos na Infraestrutura | 🔴 CRÍTICA | 5h | ✅ CRUD básico funcional, implementa interfaces |
| **F02.03** | Configurar Injeção de Dependência (Program.cs) | 🔴 CRÍTICA | 2h | ✅ DI registrada para Repositórios e Serviços |
| **F02.04** | Criar `ServicoUsuario` na Aplicação (Cadastro e Validação) | 🔴 CRÍTICA | 3h | ✅ Orquestra criação de usuário |
| **F02.05** | Validar obrigatoriedade de contato de emergência (Aplicação) | 🔴 CRÍTICA | 2h | ✅ Exceção lançada se sem contatos |
| **F02.06** | Criar DTOs para Usuário e Contato (Aplicação) | 🟠 ALTA | 2h | ✅ DTOs mapeados com AutoMapper |
| **F02.07** | Testes de integração (Infraestrutura + Domínio) | 🟠 ALTA | 4h | ✅ Cobertura > 80% de Repositórios |

**Total Sprint 2:** 21 horas

---

## 📅 Sprint 3 - Check-in e Histórico

**Duração:** 48 horas  
**Objetivo:** Implementar registro de check-in e gestão de histórico FIFO  
**Status:** 🔵 Não Iniciada

### Funcionalidades

| ID | Funcionalidade | Prioridade | Estimativa | AC |
|---|---|---|---|---|
| **F03.01** | Criar `ServicoCheckIn` na Aplicação (Registrar + Resetar 48h) | 🔴 CRÍTICA | 4h | ✅ Prazo atualizado corretamente |
| **F03.02** | Implementar lógica de Histórico FIFO (máx. 5 registros) | 🔴 CRÍTICA | 3h | ✅ Registros antigos removidos automaticamente |
| **F03.03** | Criar DTO `CheckInDTO` para requisições/respostas | 🟠 ALTA | 2h | ✅ DTO mapeado corretamente |
| **F03.04** | Criar `ServicoNotificacao` na Aplicação (Limpeza de pendentes) | 🟠 ALTA | 3h | ✅ Alertas pendentes cancelados após check-in |
| **F03.05** | Testes unitários para `ServicoCheckIn` (xUnit) | 🟠 ALTA | 4h | ✅ Cobertura > 85% |
| **F03.06** | Testes de integração (Check-in + Repositório) | 🟠 ALTA | 4h | ✅ Histórico FIFO validado |

**Total Sprint 3:** 20 horas

---

## 📅 Sprint 4 - Sistema de Alertas

**Duração:** 48 horas  
**Objetivo:** Implementar lembretes (-6h, -2h) e notificações de emergência com repetição  
**Status:** 🔵 Não Iniciada

### Funcionalidades

| ID | Funcionalidade | Prioridade | Estimativa | AC |
|---|---|---|---|---|
| **F04.01** | Criar `ServicoAlerta` (Cálculo de lembretes -6h e -2h) | 🔴 CRÍTICA | 3h | ✅ Lembretes calculados corretamente |
| **F04.02** | Implementar notificações de emergência (após vencimento) | 🔴 CRÍTICA | 4h | ✅ Emergências criadas para cada contato |
| **F04.03** | Implementar repetição a cada 6h após vencimento | 🔴 CRÍTICA | 4h | ✅ Notificação repetida até check-in |
| **F04.04** | Implementar histórico de notificações (máx. 5 por contato) | 🟠 ALTA | 3h | ✅ FIFO aplicado ao histórico de emergências |
| **F04.05** | Job/Scheduler para verificar alertas (Quartz) | 🟠 ALTA | 4h | ✅ Job executa a cada 10 minutos |
| **F04.06** | Testes para Alertas e Notificações | 🟠 ALTA | 5h | ✅ Cobertura > 85% |

**Total Sprint 4:** 23 horas

---

## 📅 Sprint 5 - API REST e Endpoints

**Duração:** 48 horas  
**Objetivo:** Expor funcionalidades via HTTP e documentar API  
**Status:** 🔵 Não Iniciada

### Funcionalidades

| ID | Funcionalidade | Prioridade | Estimativa | AC |
|---|---|---|---|---|
| **F05.01** | Criar `ControladorUsuario` (POST /usuarios, GET /usuarios/{id}) | 🔴 CRÍTICA | 3h | ✅ Endpoints retornam 200/201 |
| **F05.02** | Criar `ControladorCheckIn` (POST /check-ins, GET /historico) | 🔴 CRÍTICA | 3h | ✅ Check-in registrado via API |
| **F05.03** | Criar `ControladorNotificacao` (GET /notificacoes, GET /historico) | 🟠 ALTA | 3h | ✅ Notificações listadas corretamente |
| **F05.04** | Implementar tratamento de erros global (Middleware) | 🟠 ALTA | 3h | ✅ Erros retornam JSON estruturado |
| **F05.05** | Documentar API com Swagger/OpenAPI | 🟠 ALTA | 3h | ✅ Swagger disponível em /swagger |
| **F05.06** | Testes de integração (API + Banco + Serviços) | 🟠 ALTA | 5h | ✅ Cobertura > 75% |

**Total Sprint 5:** 20 horas

---

## 📅 Sprint 6 - Qualidade, Performance e Deploy

**Duração:** 48 horas  
**Objetivo:** Garantir qualidade e preparar para produção  
**Status:** 🔵 Não Iniciada

### Funcionalidades

| ID | Funcionalidade | Prioridade | Estimativa | AC |
|---|---|---|---|---|
| **F06.01** | Testes de cobertura completa (xUnit) | 🟠 ALTA | 6h | ✅ Cobertura global > 80% |
| **F06.02** | Validação de padrões SOLID e Clean Code | 🟠 ALTA | 4h | ✅ Sem code smells críticos |
| **F06.03** | Teste de carga e performance de alertas | 🟠 ALTA | 5h | ✅ Suporta 1000+ usuários simultâneos |
| **F06.04** | Documentação de API em Português | 🟡 MÉDIA | 3h | ✅ README.md atualizado |
| **F06.05** | Configurar CI/CD (GitHub Actions) | 🟡 MÉDIA | 6h | ✅ Pipeline testando automaticamente |
| **F06.06** | Deploy em ambiente de staging | 🟡 MÉDIA | 4h | ✅ Aplicação rodando em staging |

**Total Sprint 6:** 28 horas

---

## 🔄 Fluxo de Priorização

```
CRÍTICA (🔴)      → Sprint atual, desbloqueadores
ALTA (🟠)         → Sprint atual, após críticas
MÉDIA (🟡)        → Sprints futuras ou buffer
BAIXA (🟢)        → Nice-to-have, backlog futuro
```

---

## 📈 Roadmap Visual

```
Sprint 1  │ Domínio Puro (Entidades, Enums, Regras)
Sprint 2  │ Infraestrutura (Repositórios, DbContext, DI)
Sprint 3  │ Check-in e Histórico
Sprint 4  │ Alertas e Notificações
Sprint 5  │ API REST
Sprint 6  │ QA + Performance + Deploy
─────────────────────────────────
Semana 1  │ Semana 2  │ Semana 3  │ Semana 4+
```

---

## 🎯 Métricas de Sucesso

| Métrica | Meta | Verificação |
|---------|------|-------------|
| Cobertura de Testes | > 75% | SonarQube/OpenCover |
| Ciclo Sprint | 48 horas | Burndown chart |
| Histórico Máximo | 5 registros | Validação em BD |
| Tempo Resposta API | < 500ms | Load test |
| Disponibilidade | > 99% | Uptime monitoring |

---

## 📝 Notas de Implementação

- ✅ Todas as entidades devem ter testes unitários antes de PR.
- ✅ Código em Português (Brasil), comentários descritivos.
- ✅ Usar padrões SOLID e Result Pattern em toda parte.
- ✅ Validar ciclo 48h com testes de data/hora.
- ✅ Manter histórico FIFO automaticamente em cada operação.

---

**Próximos Passos:** Aguardar aprovação para iniciar Sprint 1.
