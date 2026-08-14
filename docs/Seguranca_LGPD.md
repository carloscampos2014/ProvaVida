# ProvaVida — Revisão de Segurança e Conformidade LGPD

Versão 1.0 — Fase 8 — Agosto de 2026

---

## 1. Autenticação e Autorização

| Item | Implementação | Status |
|---|---|---|
| Senhas armazenadas com hash seguro | BCrypt (work factor 12) via `BCrypt.Net-Next` | ✅ |
| Token JWT com expiração | `ExpirationHours: 24`, `ClockSkew: TimeSpan.Zero` | ✅ |
| Logoff invalida sessão no banco | `SessaoLogin.Invalidar()` — token marcado como inativo | ✅ |
| Token armazenado com segurança no app | `SecureStorage.Default` (Android Keystore) | ✅ |
| Endpoints protegidos por `[Authorize]` | Todos exceto `/auth/cadastro` e `/auth/login` | ✅ |
| Sem exposição de IDs sequenciais | UUIDs em todas as entidades | ✅ |

---

## 2. Proteção de Dados Pessoais (LGPD)

| Item | Implementação | Status |
|---|---|---|
| Direito à exclusão | `DELETE /conta` anonimiza todos os dados pessoais | ✅ |
| Anonimização completa | Nome, e-mail, WhatsApp, hash de senha, contato de emergência substituídos | ✅ |
| Coleta de localização com consentimento | `Permissions.RequestAsync<LocationWhenInUse>()` — solicitado antes de cada check-in | ✅ |
| Check-in funciona sem localização | `Latitude`/`Longitude` nullable — não bloqueia o fluxo | ✅ |
| Dados do contato de emergência protegidos | Anonimizados junto com os dados do usuário na exclusão | ✅ |
| Sessões invalidadas na exclusão | `InvalidarSessoesAsync` chamado antes da anonimização | ✅ |
| Finalidade declarada dos dados | App coleta apenas o necessário: nome, e-mail, WhatsApp, localização, device_id | ✅ |

---

## 3. Segurança da Infraestrutura

| Item | Implementação | Status |
|---|---|---|
| Credenciais fora do repositório | `appsettings.Development.json` no `.gitignore` | ✅ |
| Connection string via variável de ambiente | `appsettings.Development.json.example` documenta as chaves necessárias | ✅ |
| Token WhatsApp/e-mail fora do repositório | `Email:Senha`, `WhatsApp:Token` nunca commitados | ✅ |
| Chave JWT mínima de 32 chars | Validado no `appsettings.Development.json.example` | ✅ |
| API não exposta diretamente na internet (produção) | Nginx como reverse proxy + Cloudflare como CDN/WAF | ✅ |
| HTTPS obrigatório (produção) | Cloudflare Origin Certificate + modo Full Strict | ✅ |
| Banco PostgreSQL acessível apenas localmente | `pg_hba.conf` + porta não exposta externamente | ✅ (a confirmar na VM) |
| Painel Hangfire protegido em produção | `Dashboard` apenas em `Development` no `Program.cs` | ✅ |

---

## 4. Logs e Auditoria

| Item | Implementação | Status |
|---|---|---|
| Logs não contêm senhas | Serilog configurado — connection string exibida apenas como `Password=******` (Npgsql mascara) | ✅ |
| Logs não contêm tokens JWT | Nenhum log de token implementado | ✅ |
| Logs de disparo de emergência | `NotificacaoEmergencia` registrada no banco com timestamp e canal | ✅ |
| Logs de alteração de conta | Não implementado explicitamente — `atualizado_em` registra timestamp da última alteração | ⚠️ Parcial |
| Rotação de logs | `rollingInterval: Day`, `retainedFileCountLimit: 30` | ✅ |

---

## 5. Itens Pendentes / Recomendações

| Item | Prioridade | Observação |
|---|---|---|
| Implementar auditoria explícita de alterações de conta | Média | Registrar no banco quais campos foram alterados e quando (para responder a solicitações LGPD) |
| Política de privacidade e termos de uso | Alta | Documento legal a ser redigido antes da publicação nas lojas |
| Consentimento explícito na tela de cadastro | Alta | Adicionar checkbox "Li e aceito os Termos de Uso e Política de Privacidade" antes de criar conta |
| Rate limiting nos endpoints de autenticação | Média | Proteger `/auth/login` contra brute force (ex: 5 tentativas por IP por minuto) |
| Validação de força de senha no frontend | Baixa | Atualmente validado só no backend (mínimo 8 chars) — adicionar indicador visual no MAUI |
| Remoção de dados do SQLite local ao excluir conta | Média | Atualmente o SQLite local não é limpo na exclusão de conta — dados locais ficam no dispositivo |

---

## 6. Conclusão

O projeto ProvaVida implementa as principais salvaguardas de segurança e privacidade exigidas pela LGPD para um app de monitoramento de bem-estar. Os pontos críticos (autenticação segura, anonimização na exclusão, consentimento de localização, dados fora do repositório) estão implementados e validados pelos testes automatizados.

Os itens pendentes listados na seção 5 devem ser endereçados antes da publicação pública nas lojas de aplicativos (Fase 9), especialmente a política de privacidade e o consentimento explícito no cadastro.
