# ProvaVida

App mobile de check-in diário de bem-estar. O usuário registra sua presença uma vez por dia; caso fique 2 dias sem check-in, o sistema notifica automaticamente o contato de emergência cadastrado via e-mail e WhatsApp.

## Status das fases

| Fase | Status | Resultado |
|---|---|---|
| 1. Planejamento e Design | ✅ Concluída | Documentação técnica, arquitetura e protótipo de telas (Figma) aprovados |
| 2. Backend – Autenticação e Conta | ✅ Concluída | API REST com cadastro, login, logoff, alteração e exclusão de conta; Dapper + DbUp + PostgreSQL; 25 testes unitários aprovados |
| 3. App Mobile – Autenticação e Conta | ✅ Concluída | App .NET MAUI Android com Login, Cadastro e Perfil integrados à API; paleta roxa; MVVM; SecureStorage para JWT |
| 4. Backend – Check-in | 🔜 Próxima | — |
| 5. App Mobile – Check-in | ⏳ Pendente | — |
| 6. Backend – Job e Notificações | ⏳ Pendente | — |
| 7. Testes End-to-End | ⏳ Pendente | — |
| 8. Homologação (QA) | ⏳ Pendente | — |
| 9. Publicação e Lançamento | ⏳ Pendente | — |

## Stack

- **Backend:** .NET 9 (ASP.NET Core Web API), Dapper, DbUp, PostgreSQL, BCrypt, JWT
- **App Mobile:** .NET MAUI Android (a implementar)
- **Infraestrutura:** VM Oracle Cloud (OCI) — Nginx + Cloudflare (`provida-api.enzojb.com.br`)

## Estrutura

```
src/
  ProvaVida.Api/              ← Controllers, DI, middleware
  ProvaVida.Application/      ← Casos de uso, interfaces, validadores
  ProvaVida.Domain/           ← Entidades, regras de negócio
  ProvaVida.Infrastructure/   ← Dapper, DbUp, JWT, BCrypt
tests/
  ProvaVida.Application.Tests/  ← Testes unitários (xUnit, Moq, FluentAssertions)
```

## Como rodar localmente

### Pré-requisitos
- .NET 9 SDK
- PostgreSQL rodando em `localhost:5432`
- Copiar `appsettings.Development.json.example` → `appsettings.Development.json` e preencher os valores

### Executar

```powershell
# Subir a API (as migrations DbUp são aplicadas automaticamente no startup)
dotnet run --project src/ProvaVida.Api/ProvaVida.Api.csproj

# Testes unitários
dotnet test tests/ProvaVida.Application.Tests/
```

A API sobe em `http://localhost:5000`. Swagger disponível em `http://localhost:5000/swagger`.

## Documentação

- [Documentação Técnica](docs/ProvaVida_Documentacao_Tecnica.md)
- [Arquitetura](docs/ProvaVida_Arquitetura.md)
- [Cronograma](docs/ProvaVida_Cronograma.md)
