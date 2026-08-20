# Análise técnica da API ProvaVida

**Projeto analisado:** `src/ProvaVida.Api`

**Framework:** .NET 10

**Dependências principais:** ASP.NET Core, JWT Bearer, PostgreSQL/Npgsql, Dapper, DbUp, Hangfire, Serilog, Scalar e FluentValidation.

**Validação realizada:** compilação do projeto `src\ProvaVida.Api\ProvaVida.Api.csproj` concluída com sucesso.

## 1. Resumo executivo

A API possui uma separação razoável entre as camadas API, Application, Domain e Infrastructure. Os controllers são relativamente pequenos e delegam a maior parte da regra de negócio para casos de uso. A aplicação também possui autenticação JWT, persistência PostgreSQL, migrations, jobs Hangfire, logs com Serilog e testes de integração.

O problema mais grave encontrado é de segurança: `AdminController` não possui `[Authorize]` nem uma política administrativa. Dessa forma, os endpoints de métricas, painel HTML e disparo de mensagens de teste podem estar acessíveis sem autenticação. Isso deve ser tratado como bloqueador antes de qualquer exposição pública da API.

A API compila, mas ainda precisa de endurecimento operacional para produção, principalmente em autorização administrativa, proteção contra abuso dos endpoints públicos, gerenciamento de segredos, observabilidade, saúde real da infraestrutura e cobertura de testes.

## 2. Avaliação geral

| Área | Avaliação |
|---|---:|
| Organização e separação de camadas | 7/10 |
| Autenticação | 7/10 |
| Autorização | 3/10 |
| Tratamento de erros | 6/10 |
| Persistência e transações | 6/10 |
| Observabilidade | 6/10 |
| Proteção contra abuso | 3/10 |
| Testes de integração | 7/10 |
| Prontidão para produção | 5/10 |

## 3. Pontos positivos

- Projeto direcionado para `net10.0`.
- `Nullable` e `TreatWarningsAsErrors` habilitados.
- Controllers separados por responsabilidade: autenticação, conta, check-in, heartbeat e administração.
- Regras de negócio delegadas para casos de uso da camada Application.
- Uso de interfaces para repositórios, hashing, JWT e serviços externos.
- JWT configurado com validação de issuer, audience, lifetime e chave de assinatura.
- `ClockSkew` configurado como zero, evitando tolerância silenciosa adicional para tokens expirados.
- Senhas protegidas com BCrypt e work factor 12.
- Tokens de sessão e refresh token persistidos no banco.
- SQL parametrizado por meio do Dapper, reduzindo risco de SQL injection nos trechos avaliados.
- Migrations DbUp embutidas no assembly e aplicadas na inicialização.
- Hangfire separado para tarefas de inatividade e alertas.
- Handler global converte exceções conhecidas em respostas HTTP e registra erros inesperados.
- Health endpoint anônimo disponível em `/health`.
- Testes de integração cobrem cadastro, login, check-in, heartbeat e fluxos de inatividade.
- Serviços externos de notificação são substituídos por mocks nos testes de integração.

## 4. Achados críticos

### 4.1 `AdminController` está sem autorização

**Arquivo:** `src/ProvaVida.Api/Controllers/AdminController.cs`

A classe não possui `[Authorize]` e nenhum dos métodos possui uma política administrativa específica. Os seguintes recursos ficam potencialmente públicos:

- `POST /admin/testar-email`;
- `POST /admin/testar-whatsapp`;
- `POST /admin/testar-sms`;
- `POST /admin/testar-voz`;
- `GET /admin/metricas`;
- `GET /admin`.

Isso pode permitir que qualquer pessoa:

- consulte métricas e dados de usuários;
- envie mensagens usando as credenciais da aplicação;
- provoque consumo financeiro em SMS, WhatsApp e chamadas de voz;
- abuse dos provedores externos;
- use o painel administrativo sem autenticação.

**Prioridade:** bloqueador.

**Correção recomendada:** aplicar `[Authorize(Policy = "AdminOnly")]` no controller e configurar uma policy baseada em role ou claim administrativo. Também deve existir teste de integração garantindo `401` sem token e `403` com usuário comum.

### 4.2 Ausência aparente de rate limiting

Os endpoints públicos de cadastro, login e refresh não apresentam limitação de requisições.

Isso deixa a API mais vulnerável a:

- brute force de senha;
- enumeração de contas por diferenças de resposta;
- abuso do refresh token;
- criação automatizada de contas;
- negação de serviço em endpoints de autenticação.

**Prioridade:** alta.

**Correção recomendada:** adicionar rate limiting nativo do ASP.NET Core, especialmente para `/auth/login`, `/auth/cadastro` e `/auth/refresh`. Complementar com limites por IP, usuário e dispositivo quando possível.

### 4.3 Segredo e senha de desenvolvimento no repositório/configuração local

`appsettings.Development.json` contém uma senha de PostgreSQL e uma chave JWT de desenvolvimento. Mesmo que sejam valores apenas locais, esse padrão aumenta o risco de vazamento acidental e de reutilização indevida.

O arquivo `appsettings.json` também contém identificadores de remetente e configurações de serviços externos.

**Prioridade:** alta.

**Correção recomendada:**

- remover credenciais reais ou reutilizáveis dos arquivos rastreados;
- utilizar User Secrets no desenvolvimento;
- utilizar variáveis de ambiente ou Key Vault em produção;
- validar em CI que segredos não sejam commitados;
- rotacionar qualquer credencial que tenha sido exposta.

### 4.4 Dashboard Hangfire deve ser revisado

O dashboard é exposto com `Authorization = []` em Development e também existe `app.MapHangfireDashboard()` fora do ambiente de integração.

Mesmo que o ambiente de desenvolvimento não seja público, a configuração deve ser explicitamente protegida para evitar publicação acidental de um dashboard administrativo sem autenticação.

**Prioridade:** alta.

**Correção recomendada:** usar um filtro de autorização próprio, restringir por policy administrativa e avaliar se o dashboard deve ser exposto externamente.

## 5. Autenticação e autorização

### Aspectos positivos

A configuração JWT em `ServiceCollectionExtensions.AddJwtAuthentication` valida:

- issuer;
- audience;
- lifetime;
- signing key.

Também utiliza `ClockSkew = TimeSpan.Zero`.

Os controllers de conta, check-in e heartbeat estão protegidos com `[Authorize]`, enquanto cadastro, login e refresh são explicitamente anônimos.

### Pontos a melhorar

- O claim administrativo não está sendo aplicado ao controller administrativo.
- A autorização está baseada apenas em `[Authorize]` nos recursos comuns; convém definir policies nomeadas para permissões mais específicas.
- Não há evidência de proteção contra brute force de login.
- O fluxo de refresh deve ser validado para garantir rotação, revogação e uso único do refresh token.
- O endpoint de logoff extrai manualmente o header `Authorization`; seria melhor centralizar a identificação da sessão/token em uma abstração.
- A conversão de claim para `Guid` usa `Guid.Parse`, que pode gerar exceção se o token tiver claim malformado. É preferível validar com `Guid.TryParse` e retornar 401 de forma controlada.

## 6. Controllers e contrato HTTP

### Pontos positivos

Os controllers são pequenos, utilizam `CancellationToken` e retornam códigos HTTP coerentes em boa parte dos casos. O check-in também implementa idempotência, retornando 204 para novo registro e 200 para duplicado.

### Pontos a melhorar

- Os parâmetros de paginação administrativa não parecem ter limites explícitos. Valores negativos ou excessivos devem ser rejeitados ou normalizados.
- Não há evidência de limites de tamanho para payloads ou destinatários dos endpoints administrativos.
- `DateTime` é recebido diretamente no check-in e no histórico. É necessário definir claramente se os valores devem ser UTC e rejeitar datas ambíguas ou fora de uma janela aceitável.
- `DeviceId` recebe `string.Empty` quando ausente; convém validar tamanho, formato e necessidade do campo.
- Os DTOs declarados diretamente nos controllers funcionam, mas contratos maiores poderiam ser organizados em uma pasta própria para facilitar evolução e documentação.
- O painel HTML é montado dentro do controller, aumentando o acoplamento entre HTTP e apresentação. Se o painel crescer, deveria ser separado em uma aplicação administrativa ou em arquivos de apresentação dedicados.

## 7. Tratamento global de exceções

`UseGlobalExceptionHandler` trata `ValidationException`, `AppException` e exceções inesperadas.

### Pontos positivos

- Erros inesperados são registrados com stack trace no servidor.
- A mensagem retornada ao cliente para erro 500 é genérica.
- Exceções de domínio possuem status HTTP configurável.

### Riscos

- O formato de erro não segue necessariamente `ProblemDetails`, dificultando integração padronizada com clientes.
- Não há correlação explícita por request ou trace ID no payload de erro.
- Se a resposta já tiver iniciado, tentar alterar status e conteúdo pode gerar comportamento inconsistente.
- `ValidationException` sempre vira 400, mas uma política documentada de validação poderia distinguir payload inválido de regra de negócio.

**Recomendação:** adotar `ProblemDetails`, incluir `traceId` e usar middleware/filtro padronizado de erros.

## 8. Persistência, migrations e transações

A infraestrutura utiliza PostgreSQL, Npgsql e Dapper, com migrations DbUp aplicadas durante o startup.

### Pontos positivos

- SQL parametrizado nos repositórios avaliados.
- `DbConnectionFactory`, Unit of Work e repositórios estão separados.
- Migrations são embarcadas no assembly.
- Testes de integração utilizam banco configurado para testes e limpeza controlada.

### Pontos a melhorar

- Executar migrations automaticamente durante o startup pode atrasar ou impedir a inicialização se o banco estiver indisponível. Em produção, vale avaliar um passo explícito de deploy/migration.
- É importante garantir que apenas uma instância execute migrations concorrentes em ambientes com múltiplas réplicas.
- Operações que alteram múltiplas tabelas, especialmente conta, sessão e exclusão de dados, devem estar cobertas por transações.
- Índices e constraints devem ser revisados para refresh tokens, e-mail, check-ins e consultas de inatividade.
- Deve existir uma política de retenção para sessões, heartbeats, check-ins e eventos de notificação.
- Dados de sessão e refresh token devem ser tratados como material sensível. É recomendável avaliar armazenamento de hash do refresh token em vez do token em texto puro.

## 9. Jobs e notificações

Hangfire registra jobs recorrentes para verificação de inatividade e disparo de alertas.

### Pontos positivos

- Os jobs estão separados em classes da Infrastructure.
- O Hangfire usa PostgreSQL, evitando armazenamento apenas em memória.
- O fuso dos cron jobs foi explicitamente definido como UTC.

### Riscos e recomendações

- Os jobs precisam ser idempotentes para evitar alertas duplicados em reexecuções.
- Deve existir controle de tentativas, backoff e tratamento de falhas nos provedores externos.
- É necessário observar limites e custos de SMS, WhatsApp e chamadas de voz.
- O resultado do envio deve ser persistido com status e correlação suficiente para auditoria.
- Os endpoints administrativos de teste precisam de autorização, rate limiting e auditoria.
- É recomendável adicionar métricas de duração, sucesso, falha e quantidade de tentativas.

## 10. Configuração e operação

### Pontos positivos

- Serilog escreve em console e arquivo com retenção configurada.
- Há configuração separada para Development.
- O uso de `UseSystemd` é adequado para execução como serviço Linux.
- O endpoint `/health` facilita verificações básicas de deploy.

### Pontos a melhorar

O endpoint `/health` atualmente retorna `healthy` sem verificar banco, Hangfire ou provedores essenciais. Portanto, ele comprova apenas que o processo HTTP respondeu.

**Recomendação:** separar health checks de liveness e readiness:

- liveness: processo está vivo;
- readiness: banco e dependências essenciais estão disponíveis.

Também devem ser revisados:

- rotação e retenção de logs;
- mascaramento de e-mails, telefones, tokens e dados pessoais;
- correlação de requisições;
- alertas de erros 5xx;
- métricas de latência por endpoint;
- armazenamento dos arquivos de log fora da pasta da aplicação quando apropriado.

Há arquivos de log dentro de `src/ProvaVida.Api/logs`. É importante confirmar que esses arquivos não contenham tokens, credenciais, senhas, conteúdo de mensagens ou dados pessoais e que a pasta esteja excluída do controle de versão.

## 11. Testes

Os testes de integração cobrem fluxos importantes:

- cadastro e login;
- check-in novo e duplicado;
- histórico;
- heartbeat;
- acesso sem token;
- fluxos de inatividade;
- mocks para serviços externos.

### Lacunas identificadas

Não foi identificada cobertura específica suficiente para:

- todos os endpoints administrativos;
- autorização por role/policy;
- tentativa de acesso administrativo por usuário comum;
- rate limiting;
- refresh token concorrente ou reutilizado;
- expiração e revogação de sessões;
- limites de paginação;
- validação de payloads extremos;
- comportamento do health check quando o banco está indisponível;
- falhas e retries do Hangfire;
- exposição acidental de segredos em logs.

### Testes prioritários

1. `AdminController` sem token deve retornar 401.
2. Usuário comum acessando admin deve retornar 403.
3. Usuário administrativo deve conseguir acessar apenas recursos permitidos.
4. Login deve ser limitado após tentativas repetidas.
5. Refresh token usado duas vezes deve ser rejeitado conforme a política definida.
6. Refresh token revogado deve retornar 401.
7. Paginação negativa ou excessiva deve ser tratada.
8. Health readiness deve falhar quando o PostgreSQL estiver indisponível.
9. Jobs devem ser idempotentes quando executados mais de uma vez.
10. Falhas dos provedores de notificação devem gerar retry controlado e não duplicar alertas.

## 12. Prioridades de correção

### Bloqueadores

1. Proteger `AdminController` com policy administrativa.
2. Criar testes de autorização para todos os endpoints administrativos.
3. Revisar e proteger o Hangfire Dashboard.
4. Remover credenciais e segredos de arquivos rastreados ou locais compartilháveis.
5. Rotacionar credenciais que possam ter sido expostas.

### Alta prioridade

1. Adicionar rate limiting em autenticação e endpoints de teste de notificação.
2. Implementar health checks reais para PostgreSQL e dependências essenciais.
3. Validar rotação, revogação e concorrência de refresh tokens.
4. Adotar `ProblemDetails` e correlation/trace ID.
5. Definir limites de payload, paginação e destinatários.
6. Garantir idempotência e retry controlado nos jobs.
7. Revisar armazenamento de refresh tokens e dados sensíveis.

### Média prioridade

1. Melhorar validação de claims e substituir `Guid.Parse` por validação controlada.
2. Separar modelos de request/response dos controllers.
3. Adicionar métricas e logs estruturados por operação.
4. Revisar índices, constraints e retenção de dados.
5. Cobrir os cenários de falha de infraestrutura nos testes.
6. Documentar explicitamente o contrato de datas em UTC.

## 13. Conclusão

A API está bem encaminhada estruturalmente e compila sem erros. A divisão por camadas, o uso de casos de uso, a autenticação JWT, o PostgreSQL, o Hangfire e os testes de integração formam uma base sólida.

Entretanto, a ausência de autorização no `AdminController` é um problema crítico e deve ser corrigida imediatamente. Além disso, a API precisa de rate limiting, melhor gerenciamento de segredos, health checks reais, testes de autorização e maior robustez no fluxo de refresh e nos jobs.

Depois da correção dos bloqueadores e da implementação dos testes de segurança, a API estará em uma posição significativamente melhor para uma implantação controlada. No estado analisado, eu não recomendaria expor os endpoints administrativos ou publicar a API diretamente na internet sem essas correções.


---

## Resposta do time — 2026-08-20

**Concordamos com todos os achados.** Issues criadas:

| Achado | Issue | Prioridade |
|---|---|---|
| AdminController sem autorização | #145 | P0 Bloqueador |
| Hangfire Dashboard exposto | já em #145 | P0 Bloqueador |
| Rate limiting ausente | #148 | P1 Alto |
| Health check sem dependências reais | #149 | P1 Alto |
| Refresh token em texto puro / corrida | #146 | P0 Bloqueador |
| Guid.Parse pode gerar 500 | backlog | P2 Médio |
| ProblemDetails e trace ID | backlog | P2 Médio |

**Observação:** a proteção do AdminController via Nginx (tunnel SSH) foi intencional como camada de rede, mas o Copilot está correto — autorização na API é obrigatória e independente da camada de rede. Será implementado antes da exposição pública.
