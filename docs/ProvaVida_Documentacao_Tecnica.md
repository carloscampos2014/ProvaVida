# ProvaVida — Documentação Técnica

Versão 1.0 — Agosto de 2026

## 1. Visão Geral do Projeto

O ProvaVida é um aplicativo móvel que permite a usuários comprovarem que estão vivos por meio de check-ins diários. Caso o usuário deixe de realizar o check-in por dois dias consecutivos, o sistema notifica automaticamente o contato de emergência cadastrado, via e-mail e WhatsApp, ao final do segundo dia.

### 1.1 Objetivo

Oferecer uma ferramenta simples e confiável de monitoramento de bem-estar, voltada especialmente para pessoas idosas, pessoas que moram sozinhas ou que necessitam de acompanhamento à distância por familiares/responsáveis.

### 1.2 Escopo Funcional (documento base)

- Cadastrar Dados da Conta
- Alterar Dados da Conta
- Remover Dados da Conta
- Efetuar Login no App
- Efetuar Logoff no App
- Fazer Check-in no App
- Enviar Mensagem para Contato de Emergência

### 1.3 Partes Interessadas (Stakeholders)

| Papel | Descrição |
|---|---|
| Usuário titular | Pessoa que realiza o check-in diário para comprovar que está viva |
| Contato de emergência | Pessoa notificada quando o usuário deixa de fazer check-in por 2 dias |
| Equipe de desenvolvimento | Responsável pela construção, testes e manutenção do sistema |
| Equipe de operação (futuro) | Responsável por monitorar disparos, entregas de mensagens e incidentes |

## 2. Requisitos

### 2.1 Requisitos Funcionais

| ID | Requisito | Descrição |
|---|---|---|
| RF01 | Cadastrar Conta | O sistema deve permitir cadastro com nome, e-mail, WhatsApp, senha e dados do contato de emergência (nome, e-mail, WhatsApp). |
| RF02 | Alterar Conta | O sistema deve permitir que o usuário edite seus dados cadastrais e do contato de emergência. |
| RF03 | Remover Conta | O sistema deve permitir a exclusão da conta e dos dados pessoais do usuário (conforme LGPD). |
| RF04 | Login | O sistema deve autenticar o usuário por e-mail e senha, iniciando uma sessão. |
| RF05 | Logoff | O sistema deve permitir encerrar a sessão ativa do usuário. |
| RF06 | Check-in | O sistema deve registrar check-in diário contendo: ID do usuário, data/hora, localização (lat/long) e identificação do aparelho. |
| RF07 | Notificação de emergência | O sistema deve verificar diariamente os usuários sem check-in há 2 dias consecutivos e, ao final do segundo dia, executar o fluxo de três camadas: (1) verificar heartbeat recente — se houver, suspender alerta; (2) enviar push de aviso ao próprio usuário com janela de graça de 6h; (3) só após a janela sem resposta, enviar alerta ao contato de emergência via e-mail e WhatsApp. |
| RF08 | Heartbeat de sessão | O app deve enviar um sinal de presença ao backend ao ser aberto e ao recuperar conectividade, permitindo que o backend distinga inatividade real de falha de sincronização por falta de internet. |
| RF09 | Lembrete de check-in | O sistema deve notificar o usuário (push) caso ainda não tenha feito o check-in do dia. |

### 2.2 Requisitos Não Funcionais

| ID | Categoria | Descrição |
|---|---|---|
| RNF01 | Disponibilidade | O serviço de verificação diária e envio de alertas deve operar com disponibilidade mínima de 99,5%. |
| RNF02 | Segurança | Senhas armazenadas com hash (bcrypt/argon2); comunicação via HTTPS/TLS 1.2+; dados sensíveis criptografados em repouso. |
| RNF03 | Privacidade / LGPD | Coleta de localização e dados pessoais deve respeitar consentimento explícito, finalidade declarada e direito à exclusão. |
| RNF04 | Desempenho | Tempo de resposta da API abaixo de 500ms (p95) para operações de check-in e autenticação. |
| RNF05 | Escalabilidade | Arquitetura deve suportar crescimento horizontal da base de usuários sem redesenho estrutural. |
| RNF06 | Confiabilidade de notificação | Falha no envio via WhatsApp deve acionar tentativa alternativa por e-mail e registro de log de falha. |
| RNF07 | Auditoria | Toda alteração de dados de conta e todo disparo de emergência devem ser logados com timestamp. |
| RNF08 | Compatibilidade | App deve suportar Android 8+ e iOS 14+. |

## 3. Casos de Uso

### UC01 — Cadastrar Conta

**Ator:** Usuário (não autenticado)
**Fluxo:** usuário informa nome, e-mail, WhatsApp, senha e dados do contato de emergência → sistema valida unicidade do e-mail → sistema cria conta e envia confirmação.
**Exceções:** e-mail já cadastrado; dados obrigatórios ausentes.

### UC02 — Login

**Ator:** Usuário
**Fluxo:** usuário informa e-mail e senha → sistema valida credenciais → sistema gera token de sessão (JWT).
**Exceções:** credenciais inválidas; conta bloqueada.

### UC03 — Fazer Check-in

**Ator:** Usuário autenticado
**Fluxo:** usuário abre o app e toca em "Check-in" → app captura localização e identificação do aparelho → sistema registra check-in com data/hora → sistema confirma na tela.
**Regra:** apenas um check-in é necessário por dia; check-ins adicionais no mesmo dia podem ser registrados como histórico, mas não alteram a contagem de inatividade.

### UC04 — Verificação de Inatividade e Disparo de Emergência

**Ator:** Sistema (job automático — Hangfire)
**Fluxo:**
1. Rotina diária (23h50) identifica usuários sem check-in sincronizado nas últimas 48h
2. Para cada usuário: verifica se houve heartbeat nas últimas 24h
   - Com heartbeat: usuário está ativo com o app (provável falta de internet para sincronizar) — suspende alerta e registra ocorrência
   - Sem heartbeat: avança para o passo 3
3. Envia push notification ao próprio usuário ("Não detectamos seu check-in. Está tudo bem?") e registra com status `aguardando_resposta` e `janela_expira_em` = agora + 6h
4. Aguarda a janela de graça:
   - Usuário abre o app (heartbeat ou sync recebido): cancela alerta, atualiza status para `cancelado`
   - Janela expira sem resposta: envia e-mail + WhatsApp ao contato de emergência, atualiza status para `disparado`
5. Não reenvia alerta no mesmo ciclo (verifica registro existente antes de disparar)

### UC05 — Alterar / Remover Dados da Conta

**Ator:** Usuário autenticado
**Fluxo:** usuário acessa "Meus Dados" → edita ou solicita exclusão → sistema confirma ação (para exclusão, com confirmação adicional de senha) → sistema aplica alteração ou anonimiza/exclui os dados.

## 4. Segurança e Privacidade

- Autenticação via JWT com expiração e renovação de token
- Senhas com hash (bcrypt/argon2), nunca armazenadas em texto plano
- Comunicação cliente-servidor exclusivamente via HTTPS
- Consentimento explícito para coleta de localização (LGPD)
- Direito à exclusão de dados (RF03) implementado com exclusão ou anonimização
- Logs de auditoria para alterações de conta e disparos de emergência

## 5. Próximos Passos

1. Validar este documento com os stakeholders do projeto
2. Produzir protótipo navegável (Figma) das telas principais
3. Detalhar contratos de API (OpenAPI/Swagger) para cada endpoint
4. Definir provedor de WhatsApp Business API e realizar prova de conceito de envio
5. Elaborar política de privacidade e termo de consentimento (LGPD)
