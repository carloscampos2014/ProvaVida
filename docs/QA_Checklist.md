# ProvaVida — Checklist de Aceitação (QA)

Versão 1.0 — Fase 8 — Agosto de 2026

Este documento registra os testes de aceitação manuais executados no dispositivo físico Android (Redmi/MIUI, Android 15) com a API em ambiente de desenvolvimento.

---

## Ambiente de Teste

| Item | Valor |
|---|---|
| Dispositivo | Redmi (tapas_global), Android 15 |
| APK | `com.companyname.provavida.maui-Signed.apk` (Debug, net10.0-android) |
| API | `http://localhost:5182` via `adb reverse tcp:5182 tcp:5182` |
| Banco | `provavida_dev` (PostgreSQL local via WSL2) |
| Data | Agosto de 2026 |

---

## Checklist de Aceitação

### Autenticação e Conta

| # | Cenário | Resultado Esperado | Status |
|---|---|---|---|
| A01 | App instala via APK direto | Instala sem erro | ✅ Aprovado |
| A02 | App abre após instalação | Tela de Login exibida, sem crash | ✅ Aprovado |
| A03 | Cadastro — passo 1 (dados pessoais) | Campos validados, avança para passo 2 | ⏳ Pendente — validar com API em produção |
| A04 | Cadastro — passo 2 (contato emergência) | Conta criada, navega para Login | ⏳ Pendente |
| A05 | Login com credenciais válidas | Navega para tela de Check-in | ⏳ Pendente |
| A06 | Login com credenciais inválidas | Mensagem "E-mail ou senha incorretos." | ✅ Aprovado (mensagem exibida corretamente) |
| A07 | Login sem internet | Mensagem "Sem conexão com o servidor." | ✅ Aprovado |
| A08 | Editar perfil | Dados atualizados, alert "Salvo" | ⏳ Pendente |
| A09 | Logoff | Volta para Login, token inválido | ⏳ Pendente |
| A10 | Excluir conta — senha correta | Dados anonimizados, app volta para Login | ⏳ Pendente |
| A11 | Excluir conta — senha incorreta | Mensagem de erro, conta mantida | ⏳ Pendente |

### Check-in

| # | Cenário | Resultado Esperado | Status |
|---|---|---|---|
| C01 | Check-in com localização permitida | 204 na API, histórico atualizado | ⏳ Pendente |
| C02 | Check-in com localização negada | Check-in registrado sem lat/long | ⏳ Pendente |
| C03 | Check-in offline | Gravado no SQLite local, sincroniza ao reconectar | ⏳ Pendente |
| C04 | Check-in duplicado (mesmo dia) | Botão desabilitado, "já registrado" | ⏳ Pendente |
| C05 | Histórico da semana | 7 dias exibidos com indicadores corretos | ⏳ Pendente |
| C06 | Sequência exibida corretamente | Contador de dias consecutivos correto | ⏳ Pendente |

### Heartbeat e Notificações

| # | Cenário | Resultado Esperado | Status |
|---|---|---|---|
| H01 | Heartbeat ao abrir o app | `POST /heartbeat` 204 nos logs da API | ⏳ Pendente |
| H02 | Heartbeat ao recuperar internet | Retry automático ao conectar | ⏳ Pendente |
| H03 | Lembrete diário às 20h | Notificação push local exibida | ⏳ Pendente |
| H04 | Lembrete cancelado após check-in | Notificação não disparada se check-in feito | ⏳ Pendente |

### Fluxo de Inatividade (via testes automatizados)

| # | Cenário | Resultado Esperado | Status |
|---|---|---|---|
| I01 | 48h sem check-in, com heartbeat | Notificação suspensa (status heartbeat_ativo) | ✅ Validado via testes de integração |
| I02 | 48h sem check-in, sem heartbeat | E-mail de aviso enviado ao usuário | ✅ Validado via testes de integração |
| I03 | Janela de 6h expirada | E-mail + WhatsApp ao contato de emergência | ✅ Validado via testes de integração |
| I04 | WhatsApp falha, e-mail OK | E-mail enviado mesmo assim (fallback) | ✅ Validado via testes de integração |

---

## Bugs Encontrados

| # | Descrição | Gravidade | Status |
|---|---|---|---|
| B01 | APK com `EmbedAssembliesIntoApk=false` causa crash imediato | Alta | ✅ Corrigido (flag ativada) |
| B02 | `Application.MainPage` obsoleto no .NET 10 — warning em runtime | Baixa | ✅ Corrigido (`Windows[0].Page`) |
| B03 | `Frame` obsoleto no .NET 10 — 99 warnings de compilação | Baixa | ✅ Corrigido (substituído por `Border`) |

---

## Pendências para Fase 9 (Pós-Deploy em Produção)

Os cenários marcados como "⏳ Pendente" serão revalidados após o deploy da API na VM Oracle Cloud com a URL de produção (`https://provida-api.enzojb.com.br`), que é quando o APK Release poderá ser testado de ponta a ponta sem necessidade de `adb reverse`.
