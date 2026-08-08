---
inclusion: auto
---

# Padrões específicos — ProvaVida

## Stack obrigatória

- **Backend:** .NET (ASP.NET Core Web API), C#, PostgreSQL, Entity Framework Core, Hangfire (job agendado)
- **App Mobile:** React Native (ou Flutter) com SQLite local (offline-first)
- **Autenticação:** JWT + BCrypt/Argon2
- **E-mail:** SMTP (SendGrid, Amazon SES ou provedor compatível)
- **WhatsApp:** WhatsApp Business API (Meta) ou Twilio
- **Testes backend:** xUnit, FluentAssertions, Testcontainers
- **Validação:** FluentValidation
- **Infraestrutura:** VM Oracle Cloud (OCI) — Nginx + .NET (Kestrel) + PostgreSQL já provisionados

## Estrutura do backend (ASP.NET Core)

```
src/
  ProvaVida.Api/              ← Controllers, Program.cs, configuração de DI
  ProvaVida.Application/      ← Casos de uso, serviços, interfaces
  ProvaVida.Domain/           ← Entidades, regras de negócio, value objects
  ProvaVida.Infrastructure/   ← EF Core, repositórios, jobs Hangfire, integrações (e-mail, WhatsApp)
tests/
  ProvaVida.Application.Tests/
  ProvaVida.IntegrationTests/
```

## Estrutura do app mobile

```
mobile/
  src/
    screens/          ← Telas (Login, Cadastro, CheckIn, Perfil)
    components/       ← Componentes reutilizáveis
    services/         ← Chamadas à API REST
    storage/          ← SQLite local (check-ins offline, dados do usuário)
    navigation/       ← Navegação (React Navigation ou equivalente)
    hooks/            ← Custom hooks
    utils/
```

## Modelo de dados principal

| Entidade | Principais atributos |
|---|---|
| Usuario | id, nome, email, whatsapp, senha_hash, status, contato_emergencia_nome, contato_emergencia_email, contato_emergencia_whatsapp, criado_em, atualizado_em |
| CheckIn | id, usuario_id (FK), data_hora, latitude, longitude, device_id |
| NotificacaoEmergencia | id, usuario_id (FK), data_disparo, canal (email/whatsapp), status_envio |
| SessaoLogin | id, usuario_id (FK), token, criado_em, expira_em, ativo |

## Regras de negócio obrigatórias

- Exclusão de conta: remover ou anonimizar todos os dados do usuário (LGPD).
- Check-in gravado localmente primeiro (offline-first); sincronizado com o backend quando houver conexão.
- Job diário (Hangfire) verifica usuários com 2+ dias sem check-in e envia alerta ao contato de emergência.
- Mesmo check-in não pode ser duplicado no backend (idempotência via `id_local` no payload).
- Token de sessão armazenado em keychain/keystore seguro no dispositivo (nunca em texto plano).
- Localização é coletada com consentimento explícito; check-in sem localização deve ser permitido (não bloquear).

## Regras gerais

- Sem credenciais, segredos ou connection strings no código ou repositório.
- Usar `appsettings.json` + variáveis de ambiente para configurações sensíveis.
- Migrations via EF Core (`dotnet ef database update`).
- API roda via Kestrel em porta interna (`127.0.0.1:5000`); Nginx cuida de TLS/HTTPS externamente.
- Warnings tratados como erros no build da API.
