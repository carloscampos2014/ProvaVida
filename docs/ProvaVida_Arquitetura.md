# ProvaVida — Arquitetura do Sistema

Versão 1.0 — Agosto de 2026

## 1. Visão Geral da Arquitetura

Arquitetura baseada em aplicativo móvel (cliente) com banco de dados local próprio + backend hospedado em VM própria na Oracle Cloud Infrastructure (OCI), com Cloudflare como CDN/proxy DNS e terminação TLS externa, Nginx como reverse proxy interno, API REST em .NET e banco de dados PostgreSQL (ambiente já provisionado). O app mobile mantém uma base local (offline-first) que sincroniza com o backend; o agendamento da verificação diária roda dentro do próprio processo .NET, e as notificações são integradas via serviços externos de e-mail e WhatsApp.

**Domínio da API:** `provida-api.enzojb.com.br` (subdomínio gerenciado no Cloudflare)

### 1.1 Ambiente já disponível (ponto de partida)

- VM na Oracle Cloud Infrastructure (OCI) — já em uso por outro projeto
- Nginx instalado e configurado (Cloudflare Origin Certificate já instalado, firewall OCI já configurado)
- .NET runtime instalado
- PostgreSQL instalado
- Domínio `enzojb.com.br` gerenciado no Cloudflare (proxy e SSL Full Strict já ativos para o projeto existente)

### 1.2 Diagrama de Componentes (representação textual)

```
[App Mobile (Android/iOS)]
   ├─ [Banco de dados local (SQLite)]  ← check-ins e dados de sessão gravados primeiro localmente
   └─ [Camada de sincronização]
         ↓ HTTPS
   [Cloudflare — provida-api.enzojb.com.br]  ← terminação TLS, WAF, DDoS protection
         ↓ HTTPS (Cloudflare Origin Certificate)
   [Nginx (reverse proxy interno — VM OCI)]
         ↓ HTTP (127.0.0.1:5000)
   [API REST .NET (Kestrel)]  →  [PostgreSQL]

[API .NET]  →  [Job Agendado in-process (Hangfire)]  →  [Serviço de Notificação]  →  [E-mail (SMTP)] e [WhatsApp Business API]

[API .NET]  →  [Serviço de Autenticação (JWT)]

[App Mobile]  →  [Serviço de Geolocalização nativo do dispositivo]
```

## 2. Stack Tecnológica

| Camada | Tecnologia | Observação |
|---|---|---|
| App Mobile | React Native (ou Flutter) | Um único código-base para Android e iOS, acesso nativo a GPS e push notifications |
| Banco Local (Mobile) | SQLite (via `expo-sqlite`/`react-native-sqlite-storage` ou `sqflite` no Flutter) | Armazena check-ins e dados de sessão localmente, permitindo uso offline-first |
| Backend / API | .NET (ASP.NET Core Web API) | Já instalado na VM Oracle Cloud |
| DNS / CDN / Proxy | Cloudflare | Gerencia o domínio `enzojb.com.br`; subdomínio `provida-api.enzojb.com.br` com proxy ativo (modo Full Strict); WAF, DDoS protection e rate limiting incluídos |
| TLS (externo) | Cloudflare — certificado automático | TLS terminado no Cloudflare; certificado gerenciado pelo próprio Cloudflare |
| TLS (Cloudflare → VM) | Cloudflare Origin Certificate | Certificado gratuito emitido no painel Cloudflare, válido 15 anos, instalado no Nginx da VM; garante criptografia ponta a ponta mesmo entre Cloudflare e a VM |
| Servidor Web / Proxy | Nginx | Já instalado na VM; reverse proxy para a API .NET (Kestrel `127.0.0.1:5000`) + terminação TLS com Cloudflare Origin Certificate |
| Banco de Dados | PostgreSQL | Já instalado na VM |
| Agendador | Hangfire (in-process, dentro da própria API .NET) | Evita depender de serviço de nuvem externo; roda a rotina diária de verificação de inatividade; painel web embutido para monitoramento dos jobs |
| E-mail | SMTP (ex.: SendGrid, Amazon SES ou outro provedor SMTP) | Envio de e-mails transacionais e de alerta |
| WhatsApp | WhatsApp Business API (Meta) / Twilio | Envio de mensagens ao contato de emergência |
| Autenticação | JWT + BCrypt/Argon2 (via ASP.NET Identity ou implementação própria) | Sessões stateless e senhas protegidas |
| Infraestrutura | VM única na Oracle Cloud (OCI) | Hospedagem atual; ver considerações de escalabilidade abaixo |
| Observabilidade | Serilog + arquivo/console, com opção futura de integração a Grafana/Loki ou OCI Logging | Monitoramento de falhas em envios e disponibilidade |

### 2.1 Considerações específicas do ambiente Cloudflare + Nginx + .NET + PostgreSQL

- **Cloudflare:** atua como DNS, CDN, WAF e terminação TLS pública. O subdomínio `provida-api.enzojb.com.br` deve ter o proxy do Cloudflare ativado (ícone laranja). Usar modo SSL **Full (Strict)** nas configurações do Cloudflare para garantir criptografia até a VM.
- **Cloudflare Origin Certificate:** certificado gratuito emitido no painel Cloudflare (Validity: 15 anos), instalado no Nginx da VM. Não é reconhecido por browsers diretamente — funciona exclusivamente quando o tráfego passa pelo Cloudflare.
- **Firewall da OCI:** restringir regras para aceitar conexões nas portas 80/443 **somente dos IPs da Cloudflare** ([lista oficial](https://www.cloudflare.com/ips/)). Isso garante que a VM não seja acessível diretamente, forçando todo tráfego a passar pelo Cloudflare.
- **Nginx:** reverse proxy do Kestrel (`proxy_pass http://127.0.0.1:5000`), configurado para repassar cabeçalhos reais do cliente (`X-Forwarded-For`, `X-Forwarded-Proto`, `CF-Connecting-IP`). TLS configurado com o Cloudflare Origin Certificate.
- **Kestrel:** API ASP.NET Core roda em porta interna (`127.0.0.1:5000`), acessível apenas localmente — nunca exposta diretamente.
- Recomenda-se rodar a API como serviço `systemd` (com `Restart=always`) para garantir resiliência a falhas do processo.
- **Hangfire** recomendado para o job diário de verificação — painel web embutido útil para monitorar disparos de emergência.
- Usar `appsettings.json` + variáveis de ambiente (ou `dotnet user-secrets` em dev) para strings de conexão do PostgreSQL e credenciais de e-mail/WhatsApp — nunca hardcoded.
- Migrations de banco via **Entity Framework Core** (`dotnet ef database update`) garantem versionamento do schema do PostgreSQL já instalado.

## 3. Modelo de Dados (entidades principais)

| Entidade | Principais atributos |
|---|---|
| Usuario | id, nome, email, whatsapp, senha_hash, status, contato_emergencia_nome, contato_emergencia_email, contato_emergencia_whatsapp, criado_em, atualizado_em |
| CheckIn | id, usuario_id (FK), data_hora, latitude, longitude, device_id |
| NotificacaoEmergencia | id, usuario_id (FK), data_disparo, canal (email/whatsapp), status_envio |
| SessaoLogin | id, usuario_id (FK), token, criado_em, expira_em, ativo |

> Os dados do contato de emergência ficam desnormalizados na própria tabela `Usuario` (colunas prefixadas `contato_emergencia_*`), já que é sempre um único contato por usuário — evita join extra no fluxo mais crítico do sistema (leitura para disparo de notificação). Se no futuro for necessário suportar múltiplos contatos de emergência por usuário, essa decisão precisa ser revisitada e migrada para uma tabela relacionada.

## 3.1 Banco de Dados Local (App Mobile)

O app mantém uma base local (SQLite) para funcionar de forma offline-first, principalmente no fluxo de check-in — o mais crítico e o que precisa funcionar mesmo sem internet no momento do uso.

| Entidade local | Principais atributos | Finalidade |
|---|---|---|
| UsuarioLocal | id, nome, email, whatsapp, contato_emergencia_nome, contato_emergencia_email, contato_emergencia_whatsapp, token_sessao | Cópia dos dados do usuário logado, para exibir perfil e permitir uso do app sem depender de chamada de rede a cada tela |
| CheckInLocal | id_local, usuario_id, data_hora, latitude, longitude, device_id, sincronizado (bool), tentativas_sincronizacao | Registro do check-in feito no dispositivo; gravado localmente primeiro e depois enviado ao backend |

### Estratégia de sincronização

1. **Check-in:** ao tocar em "Check-in", o app grava imediatamente em `CheckInLocal` com `sincronizado = false` e mostra confirmação ao usuário na hora — a experiência não depende da rede.
2. **Envio ao backend:** em paralelo (ou assim que houver conectividade), o app tenta enviar o registro para a API `.NET`. Em caso de sucesso, marca `sincronizado = true`; em caso de falha, mantém `false` e tenta novamente (retry em background, ex.: ao abrir o app ou via job periódico local).
3. **Fonte da verdade:** o backend (PostgreSQL) é sempre a fonte da verdade para a rotina de verificação de inatividade — o app local serve para UX responsiva e tolerância a falhas de rede, não substitui o registro no servidor.
4. **Dados de conta:** nome, e-mail, WhatsApp e contato de emergência são cadastrados/editados via API (exigem conexão) e depois espelhados no `UsuarioLocal` para exibição rápida offline.
5. **Conflitos:** como o check-in é um evento de "criação" (não edição), não há necessidade de resolução de conflitos complexa — apenas garantir que o mesmo check-in não seja duplicado no backend (idempotência via `id_local` enviado no payload).

## 4. Rotina de Verificação de Inatividade

Job agendado (Hangfire/Quartz.NET, in-process na API .NET) executa diariamente (ex.: 23h50) e consulta usuários cujo último check-in tenha ocorrido há 48h ou mais. Para cada usuário identificado, o sistema verifica se já existe notificação de emergência disparada no ciclo atual; caso não exista, envia mensagem por e-mail e WhatsApp ao contato de emergência e registra o disparo em `NotificacaoEmergencia`, evitando duplicidade em execuções subsequentes.

> Observação: por rodar in-process na própria API, é importante que o serviço `systemd` da aplicação tenha `Restart=always`, para que o job não deixe de disparar em caso de falha momentânea do processo.

## 5. Segurança e Privacidade

- Autenticação via JWT com expiração e renovação de token
- Senhas com hash (bcrypt/argon2), nunca armazenadas em texto plano
- Comunicação cliente-servidor exclusivamente via HTTPS
- Consentimento explícito para coleta de localização (LGPD)
- Direito à exclusão de dados implementado com exclusão ou anonimização
- Logs de auditoria para alterações de conta e disparos de emergência
- Dados sensíveis no banco local do app (SQLite) devem ser protegidos: token de sessão armazenado em keychain/keystore seguro (não em texto plano no SQLite), e considerar criptografia do arquivo local (ex.: SQLCipher) caso o dispositivo seja comprometido

## 6. Considerações de Escalabilidade (cenário atual: VM única)

- No cenário atual (VM única na OCI com Nginx + .NET + PostgreSQL no mesmo servidor), a escalabilidade inicial é vertical: aumentar CPU/RAM/disco da VM conforme a base de usuários cresce. Isso é suficiente para um volume inicial moderado de usuários.
- Pontos de atenção nesse modelo:
  - Backend e banco de dados competem pelos mesmos recursos da VM — monitorar uso de CPU/memória do PostgreSQL separadamente do .NET.
  - Fazer backup periódico do PostgreSQL (`pg_dump` agendado) e, se possível, manter os backups fora da própria VM.
  - Configurar `ufw`/regras de firewall da OCI para expor apenas as portas 80/443 (Nginx) publicamente, mantendo PostgreSQL e Kestrel acessíveis somente localmente.
- Caminho de evolução futura, caso o volume cresça:
  - Separar o banco de dados para uma instância própria (ou serviço gerenciado), mantendo a API na VM atual.
  - Mover o job de verificação para um processo/worker separado do processo web, caso o volume de check-ins/notificações justifique.
  - Adicionar um load balancer + múltiplas VMs de API atrás do Nginx, caso seja necessário escalar horizontalmente.
- Esses passos não são necessários no lançamento inicial, mas vale já estruturar o código (camadas bem separadas, sem estado em memória entre requisições) para facilitar essa evolução sem redesenho completo.

## 7. Deploy e Operação na VM (Oracle Cloud)

A VM já está em uso por outro projeto, com Nginx, Cloudflare Origin Certificate e firewall OCI configurados. Para o ProvaVida, as adições necessárias são mínimas:

- **Novo server block no Nginx:** adicionar configuração para `provida-api.enzojb.com.br` com `proxy_pass http://127.0.0.1:5001` (porta diferente do projeto existente), reusando o Cloudflare Origin Certificate já instalado.
- **Cloudflare:** adicionar registro A `provida-api` apontando para o IP da VM, com proxy ativo. Nenhuma alteração nas configurações de SSL/TLS ou firewall é necessária.
- **Novo unit file systemd:** serviço dedicado para o ProvaVida com `Restart=always`, `Environment=ASPNETCORE_ENVIRONMENT=Production` e a porta interna configurada (`ASPNETCORE_URLS=http://127.0.0.1:5001`).
- **Publicação da API:** `dotnet publish` gerando build de release; deploy via SSH/rsync + restart do serviço `systemd`.
- **PostgreSQL:** criar banco de dados e usuário/role dedicados para o ProvaVida (não compartilhar com o projeto existente). String de conexão via variável de ambiente.
- **CI/CD (sugestão):** GitHub Actions buildando o projeto, rodando testes e fazendo deploy via SSH + restart do serviço `systemd`.
- **Logs:** Serilog gravando em arquivo rotacionado próprio; `journalctl -u provida-api` para logs do serviço.
