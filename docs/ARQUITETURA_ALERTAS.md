# 🔔 Arquitetura de Serviço de Alertas - Esclarecimento

## ❓ Pergunta: O serviço de alertas é SEPARADO da app do usuário?

**Resposta: SIM, apartado, MAS com ligações em 2 pontos:**

### 🎯 **Resposta Visual (Ultra-Rápida)**

```
APP DO USUÁRIO (Webapp/MAUI)    │    SERVIÇO DE ALERTAS (Background)
                                 │
Acionado por:                   │    Acionado por:
- Clique do usuário             │    - Relógio (a cada 10min)
- Requisição HTTP               │    - Quartz Job
                                 │
Quando executa:                 │    Quando executa:
- Quando usuário acessa         │    - 24/7 no servidor
- Esporádico                    │    - Contínuo
                                 │
Escreve no BD:                  │    Escreve no BD:
✍️ CheckIn                       │    ✍️ Notificacoes
✍️ Contatos                      │
✍️ Usuario.Status               │    Lê do BD:
                                 │    👁️ Usuarios
Lê do BD:                        │    👁️ CheckIns
👁️ Notificacoes                 │    👁️ Contatos
👁️ Seus dados                   │    👁️ Notificacoes anteriores
                                 │
                        ┌────────┴────────┐
                        │ LIGAÇÃO BANCO   │
                        │ (compartilhado) │
                        └────────┬────────┘
                                 │
                        ┌────────┴────────┐
                        │LIGAÇÃO SIGNALR │
                        │(push real-time)│
                        └─────────────────┘
```

---

## 📊 **Tabela Comparativa Ultra-Claras**

| Característica | APP DO USUÁRIO | SERVIÇO DE ALERTAS |
|--|--|--|
| **Tipo** | Interativo | Autônomo |
| **Trigger** | Usuário clica | Tempo/Scheduler |
| **Roda onde** | Browser/Device | Servidor |
| **Roda quando** | Sempre que acesso | 24/7 background |
| **Acionado por** | HTTP request | Quartz Job |
| **Frequência** | Esporádica | A cada 10min |
| **ESCREVE BD** | CheckIn, Contatos | Notificacoes |
| **LÊ BD** | Notificacoes | Usuarios, CheckIns, Contatos |
| **Comunica com Serviço** | Via BD + SignalR | Via BD + SignalR |
| **Exemplo** | "Fazer check-in agora" | "Verificar se alguém venceu" |

---

## ✅ **AS REGRAS CORRETAS DE ALERTAS**

```
PARA USUÁRIO:
├─ Dia do check-in às 08:00 → Email + Push + WhatsApp
│  Mensagem: "Bom dia! Lembre-se do check-in"
│
└─ Dia do check-in às 14:00 → Email + Push + WhatsApp
   Mensagem: "Boa tarde! Não esqueça do check-in"

PARA CONTATOS (após 48h vencido):
├─ T+48h → Email + WhatsApp (Tentativa 1/5)
│  Mensagem: "⚠️ ALERTA! [Nome] não se manifestou há 48h!"
│
├─ T+54h (6h depois) → Email + WhatsApp (Tentativa 2/5)
│
├─ T+60h (12h depois) → Email + WhatsApp (Tentativa 3/5)
│
├─ T+66h (18h depois) → Email + WhatsApp (Tentativa 4/5)
│
├─ T+72h (24h depois) → Email + WhatsApp (Tentativa 5/5)
│
└─ T+78h+ → PARA! Máximo de alertas atingido ⛔
```

---

## 🏗️ Diagrama da Arquitetura Completa

```
┌─────────────────────────────────────────────────────────────────────┐
│                          APLICAÇÃO DO USUÁRIO                       │
│                                                                       │
│  ┌──────────────────┐         ┌──────────────────────────────┐     │
│  │  Webapp / MAUI   │         │  API REST .NET               │     │
│  │  (React/Vue)     │◄────────┤  Controllers                 │     │
│  │                  │         │  • POST /check-in            │     │
│  │ • Dashboard      │         │  • GET /notifications        │     │
│  │ • Fazer Check-in │         │  • GET /contatos             │     │
│  │ • Ver Notifs     │         └──────────────────────────────┘     │
│  └──────────────────┘                    ▲                          │
│                                          │                          │
└──────────────────────────────────────────┼──────────────────────────┘
                                           │
                                    Persiste dados
                                    (SaveChanges)
                                           │
                    ┌──────────────────────┴──────────────────────┐
                    │                                              │
                    ▼                                              ▼
         ┌─────────────────────┐                      ┌──────────────────────┐
         │   Banco de Dados    │                      │ SERVIÇO DE ALERTAS    │
         │                     │                      │ (Background Job)     │
         │ • Usuarios          │◄─────Read only───────┤                      │
         │ • CheckIns          │                      │ • Quartz Scheduler   │
         │ • Contatos          │                      │ • Executa a cada 10m │
         │ • Notificacoes      │                      │                      │
         └─────────────────────┘                      └──────────────────────┘
                    ▲                                         │
                    │                                         │
                    │                      ┌──────────────────┘
                    │                      │
                    │         ┌────────────┴──────────────┐
                    │         │                           │
            Write Notificacoes│                           ▼
                    │         │                  ┌─────────────────┐
                    │         │                  │  Serviços Externos
                    │         │                  │                 │
                    │         │                  │ • Email (SMTP)  │
                    │         │                  │ • WhatsApp API  │
                    │         │                  │ • Push Noti.    │
                    │         │                  └─────────────────┘
                    │         │                           │
                    │         │      ┌────────────────────┘
                    │         │      │
                    │         │      ▼
                    └─────────┴──────────────────────►
                     Feedback: Marcar notif como enviada
```

---

## 🔄 Fluxo Completo: Do Check-in ao Alerta

### **Cenário 1: Check-in REALIZADO (Reseta contador)**

```
TEMPO: T0 (Agora)
│
├─► 1. Usuário acessa App/Webapp
│   └─ Clica em "Fazer Check-in"
│
├─► 2. Requisição chega na API
│   └─ POST /api/check-in
│
├─► 3. ServicoCheckIn valida e persiste
│   ├─ Cria novo CheckIn com data atual
│   ├─ Calcula: DataProximoVencimento = Agora + 48h
│   └─ Salva no banco
│
├─► 4. ServicoNotificacao limpa alertas antigos
│   ├─ Cancela lembretes pendentes (-6h, -2h)
│   ├─ Cancela emergências pendentes
│   └─ Atualiza status para "CANCELADA"
│
└─► 5. API retorna sucesso ao Usuário
    └─ Webapp mostra: "Check-in realizado! Próximo: 48h"

╔════════════════════════════════════════════╗
║ RESULTADO: Contador RESET para 48 horas   ║
║ Serviço de Alertas não intervém aqui!     ║
╚════════════════════════════════════════════╝
```

---

### **Cenário 2: Notificações PARA USUÁRIO (às 8h e 14h)**

```
TEMPO: T0 (Dia do Check-in às 00:00)
│ Usuário faz check-in
│ DataProximoCheckIn = Hoje + 48h
│
├─► HOJE às 08:00 (Primeira notificação)
│   ├─► Serviço de Alertas roda
│   ├─► Verifica: É 8h do dia do check-in?
│   ├─► SIM! Cria Notificacao:
│   │   ├─ UsuarioId: X
│   │   ├─ TipoNotificacao: LEMBRETE_USUARIO_8H
│   │   ├─ MeioNotificacao: EMAIL, PUSH, WHATSAPP
│   │   └─ Status: PENDENTE
│   │
│   └─► Envia imediatamente ao usuário
│       ├─ Email: "Bom dia! Lembre-se do check-in hoje"
│       ├─ Push na App: "Notificação de check-in diário"
│       └─ WhatsApp: "Olá, não esqueça seu check-in!"
│
├─► HOJE às 14:00 (Segunda notificação)
│   ├─► Serviço de Alertas roda
│   ├─► Verifica: É 14h do dia do check-in?
│   ├─► SIM! Cria Notificacao:
│   │   ├─ UsuarioId: X
│   │   ├─ TipoNotificacao: LEMBRETE_USUARIO_14H
│   │   ├─ MeioNotificacao: EMAIL, PUSH, WHATSAPP
│   │   └─ Status: PENDENTE
│   │
│   └─► Envia ao usuário
│       └─ Mensagem: "Boa tarde! Ainda há tempo para o check-in"
│
╔════════════════════════════════════════════╗
║ RESULTADO: Usuário recebe 2x NESTE DIA   ║
║ Só notificações do próprio dia            ║
║ Não há mais lembretes após 14h            ║
╚════════════════════════════════════════════╝
```

---

### **Cenário 3: Notificações PARA CONTATOS DE EMERGÊNCIA (A cada 6h após 48h)**

```
TEMPO: T0 + 48h (VENCIDO! Passado o prazo)
│
├─► 1. Serviço de Alertas detecta: VENCIMENTO EXCEDIDO
│   ├─ Verifica: Agora > DataProximoCheckIn ?
│   └─ SIM! Emergência ativada
│
├─► 2. Primeiro alerta aos contatos (T0 + 48h)
│   ├─► Para cada ContatoEmergencia do usuário:
│   │   ├─ Email: "⚠️ ALERTA! Usuário não fez check-in há 48h"
│   │   ├─ WhatsApp: "EMERGÊNCIA: Contato não realizou prova de vida!"
│   │   └─ Cria Notificacao:
│   │       ├─ ContatoEmergenciaId: Z
│   │       ├─ TipoNotificacao: EMERGENCIA
│   │       ├─ Status: ENVIADA
│   │       └─ NumeroTentativas: 1
│   │
│   └─► Atualiza: ContadorEmergencia[ContatoZ] = 1/5
│
├─► 3. Próximo alerta (T0 + 54h = +6 horas)
│   ├─► Serviço verifica: ContadorEmergencia < 5?
│   ├─► SIM! Envia NOVAMENTE
│   ├─► Email e WhatsApp aos contatos
│   └─► ContadorEmergencia[ContatoZ] = 2/5
│
├─► 4. Próximo alerta (T0 + 60h = +6 horas)
│   ├─► Continua repetindo
│   └─► ContadorEmergencia[ContatoZ] = 3/5
│
├─► 5. Próximo alerta (T0 + 66h = +6 horas)
│   ├─► Continua repetindo
│   └─► ContadorEmergencia[ContatoZ] = 4/5
│
├─► 6. Próximo alerta (T0 + 72h = +6 horas)
│   ├─► ÚLTIMA notificação
│   └─► ContadorEmergencia[ContatoZ] = 5/5 ⛔
│
└─► 7. Próxima verificação (T0 + 78h)
    ├─► Serviço verifica: ContadorEmergencia < 5?
    ├─► NÃO! Máximo atingido
    └─► PARA de notificar (não envia mais)

╔════════════════════════════════════════════╗
║ RESULTADO: Contatos notificados 5x        ║
║ A cada 6 horas                            ║
║ Máximo de 48h+ com repetição cada 6h      ║
║ PAUSA após 5 tentativas                   ║
╚════════════════════════════════════════════╝
```

---

### **Cenário 4: Usuário faz Check-in durante Emergência**

```
TEMPO: T0 + 54h (Contato já recebeu 2 alertas)
│
├─► Usuário acessa App
## 🎯 **REGRAS DE ALERTAS - CORRETAS**

| Tipo | Destinatário | Quando | Frequência | Limite | 
|------|--------------|--------|-----------|--------|
| **LEMBRETE_USUARIO_8H** | Usuário | **Hoje às 08:00** (dia do check-in) | 1x | Só este dia |
| **LEMBRETE_USUARIO_14H** | Usuário | **Hoje às 14:00** (dia do check-in) | 1x | Só este dia |
| **EMERGENCIA** | Contatos | **Passado o prazo** (T0 + 48h+) | A cada 6h | Máx 5x |

---

## 📋 **Resumo Executivo: App vs Serviço de Alertas**

| Aspecto | App do Usuário | Serviço de Alertas |
|--------|-----------------|-------------------|
| **Responsabilidade** | Interface, Check-in | Verificar prazos, Criar alertas |
| **Acionado por** | Ação do usuário | Tempo (Quartz Job a cada 10min) |
| **Acesso ao BD** | Escreve: Check-in, Contatos | Lê: Check-in, Usuário, Contatos; Escreve: Notificações |
| **Comunica via** | HTTP/HTTPS | SignalR/WebSocket (push real-time) |
| **Quando executa** | Quando usuário clica | Continuamente (background) |
| **Ligação** | SIM - via BD e SignalR | SIM - lê dados da App |

---

## ✅ **Fluxo Final Correto: USUARIO + ALERTAS**

```
DIA 1 - 08:00 ─────────────────────────────────
│
├─ USUÁRIO FAZ CHECK-IN (via App/Webapp)
│  └─ SaveChanges() no BD
│     ├─ Cria CheckIn
│     └─ DataProximoCheckIn = Hoje + 48h (DIA 3 08:00)
│
├─ APP mostra: "✅ Check-in realizado! Próximo: em 48h"
│
└─────────────────────────────────────────────
  
DIA 1 - 08:00 ─────────────────────────────────
│
├─ SERVIÇO DETECTA: É 8h do dia do check-in?
│  └─ SIM!
│
├─ CRIA ALERTA para USUÁRIO:
│  ├─ TipoNotificacao: LEMBRETE_USUARIO_8H
│  ├─ Envia Email: "Bom dia! Lembre-se do check-in"
│  ├─ Envia Push via SignalR
│  └─ Envia WhatsApp
│
└─────────────────────────────────────────────

DIA 1 - 14:00 ─────────────────────────────────
│
├─ SERVIÇO DETECTA: É 14h do dia do check-in?
│  └─ SIM!
│
├─ CRIA ALERTA para USUÁRIO:
│  ├─ TipoNotificacao: LEMBRETE_USUARIO_14H
│  ├─ Envia Email: "Boa tarde! Não esqueça do check-in"
│  ├─ Envia Push via SignalR
│  └─ Envia WhatsApp
│
└─────────────────────────────────────────────

DIA 1 - 23:59 ─────────────────────────────────
│
└─ SERVIÇO executa (background job)
   ├─ Verifica alertas às 8h? ✅ Já enviado
   ├─ Verifica alertas às 14h? ✅ Já enviado
   └─ Verifica vencimento? NÃO (faltam 8 horas)

═════════════════════════════════════════════

DIA 2 (00:00 até 08:00) ────────────────────
│
└─ Sem alertas (usuário já fez check-in)

═════════════════════════════════════════════

DIA 3 - 08:00 ─────────────────────────────────
│
├─ SERVIÇO DETECTA: Passou 48h sem novo check-in?
│
├─ ATIVA EMERGÊNCIA! ⚠️
│  ├─ Cria notificações para CONTATOS:
│  │  ├─ Email: "ALERTA! Usuário não fez check-in"
│  │  ├─ WhatsApp: "EMERGÊNCIA! Contato não respondeu!"
│  │  └─ Status: ENVIADA
│  │
│  └─ Atualiza contador: Tentativa 1/5
│
└─────────────────────────────────────────────

DIA 3 - 14:00 (6h depois) ──────────────────
│
├─ SERVIÇO DETECTA: Ainda sem check-in?
│
├─ ENVIA NOVAMENTE aos CONTATOS:
│  ├─ Email e WhatsApp
│  └─ Contador: Tentativa 2/5
│
└─────────────────────────────────────────────

DIA 3 - 20:00 (12h depois) ─────────────────
│
├─ Contador: 3/5
└─ Processa...

═════════════════════════════════════════════

DIA 4 - 02:00 (18h depois) ─────────────────
│
├─ Contador: 4/5
└─ Processa...

═════════════════════════════════════════════

DIA 4 - 08:00 (24h depois) ─────────────────
│
├─ Contador: 5/5 ⛔
├─ ÚLTIMA notificação enviada
└─ PARA de notificar contatos

═════════════════════════════════════════════

DURANTE TODO ESTE TEMPO:
│
├─► SE USUÁRIO FAZER CHECK-IN:
│   ├─ App registra novo check-in
│   ├─ ServicoNotificacao CANCELA alertas
│   └─ Contatos não recebem mais alertas
│
└─► SE NÃO FAZER CHECK-IN:
    ├─ Alertas continuam (máx 5x)
    └─ Após 5 tentativas: PARA
```

---

## 🔐 **Dados Compartilhados entre App e Serviço**

### **O que a App ESCREVE no BD:**
- ✍️ `CheckIn` (novo, com DataProximoCheckIn = agora + 48h)
- ✍️ `Usuario.Status` (ATIVO, INATIVO)
- ✍️ `ContatoEmergencia` (add/remove)

### **O que o Serviço de Alertas LÊ do BD:**
- 👁️ `Usuario` (para saber quem está ativo)
- 👁️ `CheckIn` (para calcular se venceu)
- 👁️ `ContatoEmergencia` (para saber quem notificar)
- 👁️ `Notificacao` (para verificar status de anteriores)
public class NotificacaoHub
{
    onConnect() {
        // Conecta ao SignalR do backend
        this.connection.on("AlertaEmergencia", (dados) => {
            // Mostra notificação pop-up para usuário
            this.mostrarNotificacao(dados);
            
            // Ou envia para contatos via PWA Push
            this.enviarPushNativo(dados);
        });
    }
}
```

---

## ✅ RESUMO: Ligações do Serviço de Alertas

| Ligação | Tipo | Como Funciona | Exemplo |
|---------|------|---------------|---------|
| **Com App do Usuário** | Indireta | Via Banco de Dados | Serviço lê CheckIns que app criou |
| **Com App do Usuário** | Direta | Via SignalR | Serviço envia notificação push real-time |
| **Com Banco de Dados** | Read/Write | Lê usuários, escreve notificações | SELECT de usuarios, INSERT de notificacoes |
| **Com Serviços Externos** | Direto | Envia emails, WhatsApp | SMTP, Twilio API |
| **Com Contatos de Emergência** | Indireto | Via Email/WhatsApp | Serviço envia, não recebe |

---

## 📊 Arquitetura em Camadas

```
┌─────────────────────────────────────────────────────────┐
│ APRESENTAÇÃO (Webapp/MAUI)                              │
│ • Usuário clica em "Check-in"                           │
│ • Recebe notificações push real-time                    │
└──────────────────────┬──────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────┐
│ APLICAÇÃO (.NET)                                        │
│ • ServicoCheckIn (triggered pela app)                  │
│ • ServicoAlerta (triggered pelo Scheduler)             │
│ • ServicoNotificacao (comum aos dois)                  │
└──────────────────────┬──────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────┐
│ DOMÍNIO (Lógica de Negócio)                            │
│ • Regras de 48h                                        │
│ • Validações de alertas                                │
│ • Decisões de emergência                               │
└──────────────────────┬──────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────┐
│ INFRAESTRUTURA (.NET + Quartz)                         │
│ • Repositórios (CRUD)                                  │
│ • Scheduler (Job a cada 10min)                         │
│ • SignalR Hub (Push notificações)                      │
│ • SMTP/WhatsApp Client                                 │
└──────────────────────┬──────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────┐
│ BANCO DE DADOS                                          │
│ Compartilhado entre App e Serviço de Alertas           │
└─────────────────────────────────────────────────────────┘
```

---

## 🎯 Resposta Direta à sua Pergunta

### **"O serviço de alertas é algo apartado?"**

**✅ SIM, é um serviço separado** - Roda em background, independente

```
Serviço de Alertas (Apartado)     Aplicação do Usuário
├─ Roupa no servidor              ├─ Roda quando usuário acessa
├─ 24/7 verificando               ├─ Responde a requisições
├─ Quartz Job                     ├─ Webapp/MAUI
└─ A cada 10 minutos              └─ HTTP/WebSocket
```

### **"Tem ligação com a app do usuário?"**

**✅ SIM, tem 2 ligações:**

1. **Ligação 1 - Banco de Dados (Indireta)**
   - App escreve CheckIns
   - Serviço lê CheckIns
   - Serviço escreve Notificações
   - App lê Notificações

2. **Ligação 2 - SignalR (Direta em Real-time)**
   - Quando Serviço cria notificação
   - Envia push para App via WebSocket
   - App mostra popup/notificação imediato

---

## 📅 Implementação por Sprint

```
Sprint 3: ServicoNotificacao (ligação com Check-in)
├─ Quando usuário faz check-in
└─ App ordena: cancele notificações pendentes

Sprint 4: ServicoAlerta (serviço separado)
├─ Quartz Job roda independentemente
├─ Verifica prazos
├─ Cria notificações
├─ Envia via Email/WhatsApp
└─ PUSH via SignalR para App

Sprint 5: API REST
├─ Controllers expõem endpoints
├─ SignalR Hub para notificações
└─ App consome em real-time

Sprint 6: Teste E2E
├─ Validar fluxo completo
├─ User faz check-in
├─ User vence
├─ App recebe push
└─ Contatos recebem email/whatsapp
```

---

## 🔍 Exemplo Visual de Execução

```
SEGUNDA-FEIRA 10:00 - Usuário faz check-in
┌────────────────────────────────────┐
│ Webapp do Usuário                  │
│ Clica: "Fazer Check-in"            │
└────────┬─────────────────────────────┘
         │ HTTP POST /api/check-in
         ▼
┌────────────────────────────────────┐
│ API (.NET)                         │
│ ✓ ServicoCheckIn.RegistrarAsync()  │
│ ✓ Cria CheckIn + 48h               │
│ ✓ ServicoNotificacao.LimparAsync() │
│   (cancela lembretes)              │
└────────┬─────────────────────────────┘
         │ SaveChanges()
         ▼
┌────────────────────────────────────┐
│ Banco de Dados                     │
│ ✓ INSERT CheckIn                   │
│ ✓ UPDATE Notificacao (CANCELADA)   │
│ Próximo vencimento: QUARTA 10:00   │
└────────────────────────────────────┘


QUARTA-FEIRA 04:00 - Faltam 6h
┌────────────────────────────────────┐
│ Quartz Job dispara                 │
│ (Serviço de Alertas)               │
└────────┬─────────────────────────────┘
         │ SELECT usuarios WHERE vencimento <= agora+6h
         ▼
┌────────────────────────────────────┐
│ ServicoAlerta.ProcessarAlertas()   │
│ ✓ Encontrou usuário João           │
│ ✓ Cria Notificacao (LEMBRETE_6H)   │
│ ✓ Envia Email                      │
│ ✓ Envia Push via SignalR           │
└────────┬─────────────────────────────┘
         │ 1. INSERT Notificacao + UPDATE status
         │ 2. Signal.SendAsync("AlertaUsuario")
         │
         ├─► Banco de Dados (escreve)
         │
         └─► Webapp do João (recebe push real-time)
                ┌──────────────────────┐
                │ 🔔 ALERTA!           │
                │ Faltam 6h para o     │
                │ próximo check-in     │
                │ [Fazer Agora]        │
                └──────────────────────┘


QUARTA-FEIRA 10:00 - VENCIDO!
┌────────────────────────────────────┐
│ Quartz Job dispara                 │
│ (Serviço de Alertas)               │
└────────┬─────────────────────────────┘
         │ SELECT usuarios WHERE vencimento <= agora
         ▼
┌────────────────────────────────────┐
│ ServicoAlerta.CriarEmergencia()    │
│ ✓ Encontrou usuário João           │
│ ✓ Para cada ContatoEmergencia:     │
│   ├─ Cria Notificacao (EMERGENCIA) │
│   ├─ Email + WhatsApp para Contato │
│   └─ Agenda repetição em 6h        │
└────────┬─────────────────────────────┘
         │
         ├─► Banco de Dados
         │   INSERT Notificacao EMERGENCIA
         │
         ├─► Email (mae@example.com)
         │   📧 "ALERTA: João não se manifestou há 48h!"
         │
         ├─► WhatsApp
         │   💬 "ALERTA: João não se manifestou há 48h!"
         │
         └─► Webapp do João (ainda recebe push)
             🔔 ALERTA MÁXIMO! Contatos foram contatados!
```

---

**Ficou claro agora? Quer que eu detalhe mais alguma parte da arquitetura?** 🤔
