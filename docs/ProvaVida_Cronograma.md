# ProvaVida — Cronograma de Desenvolvimento

Versão 1.0 — Agosto de 2026

Estimativa para uma equipe pequena (1 dev mobile, 1 dev backend, 1 QA/PO parcial), em ciclos semanais. Duração total estimada: 12 semanas (3 meses).

## 1. Fases do Projeto

| Fase | Semanas | Entregas principais |
|---|---|---|
| 1. Planejamento e Design | 1 – 2 | Documentação técnica, protótipo de telas (UX/UI), modelagem de banco de dados, definição de arquitetura (infraestrutura já disponível: VM Oracle Cloud com Nginx, .NET e PostgreSQL) |
| 2. Backend – Autenticação e Conta | 3 – 4 | API de cadastro, login, logoff, alteração e remoção de conta; banco de dados configurado; testes unitários |
| 3. App Mobile – Autenticação e Conta | 3 – 5 | Telas de cadastro, login, edição e remoção de conta integradas à API |
| 4. Backend – Check-in | 5 – 6 | API de check-in (armazenando usuário, data/hora, localização, device); testes |
| 5. App Mobile – Check-in | 6 – 7 | Tela de check-in com captura de localização e identificação do aparelho; push de lembrete diário |
| 6. Backend – Job de Verificação e Notificações | 7 – 8 | Job agendado de verificação de inatividade; integração com serviço de e-mail e WhatsApp Business API |
| 7. Integração e Testes End-to-End | 9 – 10 | Testes integrados de todo o fluxo (cadastro → check-in → inatividade → disparo de alerta); testes de carga básicos |
| 8. Homologação (QA) e Ajustes | 10 – 11 | Correção de bugs, testes de aceitação, revisão de segurança e LGPD |
| 9. Publicação e Lançamento | 12 | Publicação nas lojas (Google Play / App Store), monitoramento pós-lançamento |

## 2. Marcos (Milestones)

| Marco | Data prevista (semana) | Critério de conclusão |
|---|---|---|
| M1 – Arquitetura aprovada | Semana 2 | Documento técnico e protótipo validados |
| M2 – Contas funcionando ponta a ponta | Semana 5 | Cadastro, login, logoff, edição e remoção operacionais em app + API |
| M3 – Check-in funcional | Semana 7 | Check-in registrando dados corretamente, com histórico visível |
| M4 – Alerta de emergência funcional | Semana 8 | Disparo automático de e-mail e WhatsApp após 2 dias sem check-in |
| M5 – Release candidate | Semana 11 | App estável, testado, pronto para lojas |
| M6 – Lançamento | Semana 12 | App publicado e monitorado em produção |

## 3. Riscos e Mitigações

| Risco | Impacto | Mitigação |
|---|---|---|
| Bloqueio/instabilidade da WhatsApp Business API | Alto — falha no alerta principal | Fallback automático para e-mail e possível SMS; monitoramento de status de entrega |
| Usuário não concede permissão de localização | Médio | Permitir check-in sem localização precisa, registrando apenas indisponibilidade, sem bloquear a funcionalidade principal |
| Falsos positivos de inatividade (ex.: app fechado, sem internet) | Médio | Permitir check-in offline com sincronização posterior; janela de tolerância antes do disparo |
| Questões de conformidade LGPD | Alto | Revisão jurídica do fluxo de consentimento e política de privacidade antes do lançamento |
