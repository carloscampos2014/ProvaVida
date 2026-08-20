# Análise dos projetos ProvaVida — exceto MAUI e API

**Projetos analisados:**

- `src/ProvaVida.Domain`
- `src/ProvaVida.Application`
- `src/ProvaVida.Infrastructure`
- `tests/ProvaVida.Application.Tests`
- `tests/ProvaVida.IntegrationTests`

**Projetos não incluídos:**

- `mobile/ProvaVida.Maui` — análise documentada separadamente.
- `src/ProvaVida.Api` — análise documentada separadamente em `.vs/ANALISE-API.md`.

**Validação realizada:**

- 75 testes unitários aprovados.
- 14 testes de integração aprovados.
- Os testes de integração dependem do PostgreSQL local configurado para `provavida_dev`.

## 1. Resumo executivo

A solução possui uma divisão de camadas coerente: o Domain concentra entidades, o Application concentra casos de uso e contratos, o Infrastructure implementa persistência e integrações externas, e os projetos de testes cobrem parte relevante dos fluxos.

A base é adequada para evolução, mas há riscos importantes em segurança de tokens, concorrência, consistência transacional, idempotência dos jobs, validação de domínio e isolamento dos testes. Os testes existentes passam, porém concentram-se nos caminhos esperados e não cobrem suficientemente concorrência, falhas de infraestrutura, autorização, reprocessamento de jobs e abuso de integrações externas.

### Avaliação geral

| Projeto/área | Avaliação |
|---|---:|
| Domain | 6/10 |
| Application | 7/10 |
| Infrastructure | 6/10 |
| Testes unitários | 7/10 |
| Testes de integração | 7/10 |
| Prontidão geral para produção | 5/10 |

## 2. Achados mais importantes

### ALTO — refresh token em texto puro e rotação vulnerável a concorrência

`src/ProvaVida.Domain/Entities/SessaoLogin.cs` mantém o refresh token diretamente. A migration `V004_RefreshToken.sql` também cria uma coluna com o valor original e um índice sobre ele.

Em caso de acesso indevido ao banco, o refresh token pode ser reutilizado. Além disso, `RefreshTokenUseCase` primeiro consulta e valida a sessão, depois invalida o objeto em memória e somente posteriormente grava a alteração. Duas requisições concorrentes podem validar o mesmo token antes de qualquer uma persistir a invalidação.

**Recomendação:** armazenar hash do refresh token, realizar consumo/invalidação de forma atômica no banco e adicionar teste concorrente de rotação e reutilização.

### ALTO — idempotência de check-in tem escopo global

`V002_CheckIn_Heartbeat.sql:11` cria `UNIQUE (id_local)`. O repositório usa `ON CONFLICT (id_local) DO NOTHING`.

Se `id_local` for único apenas por usuário ou instalação, dois usuários com o mesmo UUID serão tratados como o mesmo check-in. Isso pode gerar perda de registro.

**Recomendação:** confirmar o contrato. Se a unicidade for por usuário, usar `UNIQUE (usuario_id, id_local)` e ajustar o `ON CONFLICT`.

### ALTO — jobs podem duplicar notificações

`VerificarInatividadeUseCase` consulta janelas expiradas e depois envia notificações. A notificação não parece ser reivindicada atomicamente antes do envio. Dois workers ou uma reexecução do Hangfire podem processar o mesmo registro simultaneamente.

Os retries automáticos do Hangfire são úteis, mas sem idempotência podem causar envio duplicado de e-mail, WhatsApp, SMS ou ligação.

**Recomendação:** introduzir estado de processamento, lock/claim transacional, chave de idempotência por canal e controle de tentativas.

### ALTO — testes de integração usam banco fixo e destrutivo

`ProvaVidaWebFactory.cs` e `DatabaseCleaner.cs` usam a base local `provavida_dev` com credenciais fixas e executam `TRUNCATE ... CASCADE` antes de cada cenário.

Isso pode apagar dados de desenvolvimento e gerar interferência quando testes rodarem em paralelo ou quando mais de um processo usar a mesma base.

**Recomendação:** usar banco/schema dedicado por execução, container PostgreSQL ou desabilitar paralelismo explicitamente quando a infraestrutura compartilhada for inevitável.

### MÉDIO — `UnitOfWork` expõe APIs assíncronas, mas executa operações síncronas

`UnitOfWork.cs:30-49` chama `Open`, `BeginTransaction`, `Commit` e `Rollback` síncronos dentro de métodos que retornam `Task` concluída.

Isso pode bloquear threads durante latência ou indisponibilidade do banco e cria uma expectativa incorreta para quem chama `BeginAsync`/`CommitAsync`.

**Recomendação:** usar APIs assíncronas do Npgsql ou renomear a abstração para deixar explícito o comportamento síncrono.

## 3. Análise do Domain

### Pontos positivos

- Entidades têm setters privados.
- Fábricas estáticas deixam explícita a criação de `Usuario`, `SessaoLogin`, `CheckIn`, `Heartbeat` e `NotificacaoEmergencia`.
- `Usuario` normaliza e-mails com `ToLowerInvariant`.
- Há métodos de domínio para alteração de dados, alteração de senha, invalidação de sessão e anonimização LGPD.
- As entidades não dependem de infraestrutura.
- Datas de criação e alteração do usuário são geradas em UTC.

### Pontos a melhorar

#### Invariantes fracas em `CheckIn`

`CheckIn.Criar` aceita qualquer data, latitude, longitude e tamanho de `DeviceId`. Não há validação de:

- latitude entre -90 e 90;
- longitude entre -180 e 180;
- data futura ou muito antiga;
- identificador de dispositivo vazio ou excessivamente grande;
- `DateTimeKind` ou política de UTC.

**Recomendação:** validar no domínio ou em validator específico de entrada, mantendo a API como segunda barreira.

#### Invariantes fracas em `Heartbeat`

`Heartbeat.Criar` aceita qualquer data e não impede timestamps futuros ou fora da janela de negócio.

#### Estados representados por strings

`NotificacaoEmergencia` representa status e canal com strings. Isso permite transições inválidas e valores não previstos.

**Recomendação:** usar enums/value objects ou métodos de transição que validem o estado atual.

#### Entidade de sessão contém material sensível

`SessaoLogin` guarda JWT e refresh token completos. Mesmo que o funcionamento atual dependa disso, a exposição de uma cópia do token no banco amplia o impacto de um vazamento.

#### Comentário de EF Core sem evidência de EF neste fluxo

As entidades possuem comentários e navegação sugerindo EF Core, enquanto a implementação atual utiliza Dapper. Isso pode confundir a manutenção e indicar resquícios de uma arquitetura anterior.

## 4. Análise do Application

### Pontos positivos

- Casos de uso não dependem diretamente de controllers.
- Interfaces isolam repositórios, JWT, hashing, e-mail, SMS, voz e WhatsApp.
- Cadastro e alteração de conta usam FluentValidation.
- Login usa mensagem genérica para credenciais inválidas.
- Operações de escrita normalmente abrem transação e fazem rollback em caso de exceção.
- Exclusão de conta anonimiza dados e invalida sessões.
- O fluxo de inatividade contempla heartbeat, aviso ao usuário e alerta ao contato de emergência.
- `CancellationToken` é propagado na maioria das operações.

### Pontos a melhorar

#### Corrida no cadastro

`CadastrarUsuarioUseCase.cs:33-35` consulta se o e-mail existe antes de inserir. A constraint única protege o banco, mas uma corrida pode gerar exceção de infraestrutura em vez de uma resposta de conflito consistente.

**Recomendação:** manter a constraint única e converter a violação de unicidade para `AppException.Conflito`.

#### Validação incompleta de senha

O cadastro valida somente tamanho mínimo/máximo. A alteração de senha verifica tamanho diretamente e lança `InvalidOperationException`, o que pode virar erro 500 no middleware da API.

**Recomendação:** criar validator para `AlterarSenhaInput` e usar a mesma política de senha em cadastro e alteração.

#### Validação inexistente para check-in

`RegistrarCheckInUseCase` apenas cria e persiste o objeto. Não valida data, coordenadas, tamanho do dispositivo ou consistência dos campos.

#### Refresh token não atômico

O caso de uso implementa a intenção correta de rotação, mas a sequência leitura-validar-invalidar-inserir não impede duas requisições concorrentes.

#### Falhas parciais no fluxo de inatividade

O envio de canais é independente, mas o estado persistido precisa registrar o resultado individual de cada canal. Caso um canal seja enviado e outro falhe, o retry deve evitar duplicar o canal que já funcionou.

#### Operações N+1 no processamento de inatividade

O fluxo busca usuários inativos e realiza consultas adicionais de notificação, heartbeat e usuário para cada usuário. Em escala maior, isso pode gerar muitas conexões e consultas sequenciais.

**Recomendação:** trabalhar com lotes, projeções SQL e limite por execução.

#### Exceções de provedores podem ser devolvidas ao cliente

`TestarNotificacaoUseCase` retorna `ex.Message` no campo `Mensagem`. Mensagens de SMTP, Twilio ou APIs externas podem revelar detalhes internos.

**Recomendação:** devolver mensagem genérica, registrar detalhes internamente e associar um correlation ID.

## 5. Análise da Infrastructure

### Pontos positivos

- SQL Dapper usa parâmetros nos trechos avaliados.
- `DbConnectionFactory` centraliza criação de conexões.
- Unit of Work permite compartilhar conexão/transação nas escritas.
- Migrations estão versionadas e embarcadas como recursos.
- PostgreSQL oferece constraints e índices importantes.
- BCrypt é usado com work factor 12.
- Refresh token possui geração criptograficamente segura com `RandomNumberGenerator`.
- Jobs possuem retry automático do Hangfire.
- Integrações externas são abstraídas por interfaces.

### Persistência e migrations

#### Migrations durante inicialização

`DatabaseMigrator` executa `EnsureDatabase` e `PerformUpgrade` no startup. Isso simplifica o deploy, mas pode impedir a aplicação de iniciar quando o banco estiver temporariamente indisponível.

Em múltiplas instâncias, é necessário validar o comportamento de migrations concorrentes e o controle de locks do DbUp.

**Recomendação:** considerar um passo separado de migration no pipeline de deploy e verificar readiness antes de liberar tráfego.

#### Mapeamento por reflection

Repositórios usam `RuntimeHelpers.GetUninitializedObject` e reflection para preencher entidades com setters privados.

Isso contorna construtores e invariantes, é mais frágil diante de renomeação de propriedades e torna erros de mapeamento mais difíceis de detectar.

**Recomendação:** usar mapeamento explícito ou construtores internos de persistência.

#### Leituras e escritas usam conexões diferentes

As leituras usam `DbConnectionFactory`, enquanto as escritas usam a conexão da Unit of Work. Isso é válido se intencional, mas uma operação não deve presumir que todas as leituras participam da mesma transação.

#### Ausência de retenção evidente

Check-ins, heartbeats, sessões e notificações podem crescer indefinidamente. É recomendável definir retenção, arquivamento ou limpeza de sessões expiradas e eventos antigos.

### Segurança

- O JWT inclui `sub`, e-mail, `jti` e nome, mas não inclui claim de role administrativa.
- A chave JWT é lida de configuração, sem validação explícita de força/tamanho no Infrastructure.
- Refresh tokens são consultados e persistidos em texto puro.
- Tokens de acesso completos também são gravados em `sessoes_login`.

### Jobs

`VerificacaoInatividadeJob` e `DispararAlertaJob` registram erros e relançam a exceção, permitindo que o Hangfire aplique retry. Porém, retry sem idempotência pode repetir notificações.

Também é importante avaliar:

- limite de lotes;
- duração máxima;
- cancelamento;
- concorrência entre workers;
- lock distribuído;
- métricas de sucesso/falha;
- política de backoff.

### Notificações

#### WhatsApp Meta

`WhatsAppService` define o header de autorização em `DefaultRequestHeaders` e usa `HttpClient`. Como o cliente pode ser reutilizado, é mais seguro criar `HttpRequestMessage` e colocar o token no header da requisição, evitando estado mutável compartilhado.

A resposta de erro externa é incluída em uma `InvalidOperationException`; esse conteúdo não deve ser exposto diretamente ao cliente final.

#### Twilio

Os serviços Twilio inicializam `TwilioClient` globalmente nos construtores e registram números de telefone em nível Information. Telefones são dados pessoais e devem ser mascarados nos logs.

Os serviços também não propagam o `CancellationToken` para todas as chamadas do SDK Twilio, o que pode manter chamadas externas ativas após o cancelamento da requisição/job.

#### E-mail

`EmailService` usa HTML recebido na mensagem. Atualmente os textos são gerados internamente, mas se qualquer conteúdo de usuário for incorporado no futuro, deverá ser escapado antes de entrar no HTML.

A configuração usa `int.Parse` para porta SMTP; configurações inválidas provocam exceção em runtime em vez de falha clara de validação no startup.

## 6. Análise dos testes unitários

### Pontos positivos

- 75 testes unitários passaram.
- Há testes para entidades e diversos casos de uso.
- São usados xUnit, Moq e FluentAssertions.
- Há cobertura de cadastro, login, logoff, refresh, alteração, exclusão, check-in, heartbeat, métricas e inatividade.
- Falhas de banco e de serviços externos aparecem em alguns cenários.

### Lacunas

Não há cobertura suficiente para:

- duas rotações de refresh concorrentes;
- reutilização de refresh token após rotação;
- violação de constraint única sob corrida;
- latitude/longitude inválidas;
- timestamps futuros ou em fusos distintos;
- reexecução simultânea do job;
- duplicação parcial de canais de notificação;
- timeout e cancelamento nos provedores;
- limite de tamanho dos campos;
- retenção/limpeza de sessões e eventos.

Os testes de `TestarNotificacaoUseCase` verificam mensagens com o texto da exceção externa. Isso consolida um contrato que pode vazar detalhes internos para a API.

## 7. Análise dos testes de integração

### Pontos positivos

- 14 testes de integração passaram.
- `WebApplicationFactory` inicializa a API real.
- PostgreSQL real é usado nos fluxos.
- Serviços de e-mail e WhatsApp são substituídos por mocks.
- Cadastro/login/logoff, alteração de conta, exclusão, check-in, heartbeat e inatividade são exercitados.
- Existe teste de check-in sem token.

### Limitações

- O banco é fixo e compartilhado.
- A limpeza com `TRUNCATE ... CASCADE` é destrutiva.
- Os jobs Hangfire são desabilitados, então a execução real dos jobs não é validada.
- Não existem testes para os endpoints administrativos.
- Não há teste de autorização por role/policy.
- Não há teste de concorrência ou paralelismo.
- A integração externa real não é testada, o que é correto para testes automatizados, mas exige testes de contrato ou sandbox separados.

## 8. Prioridades recomendadas

### P0 — antes de produção

1. Corrigir autorização administrativa na API, conforme relatório `ANALISE-API.md`.
2. Proteger o Hangfire Dashboard.
3. Remover credenciais fixas dos testes e configurações compartilháveis.
4. Definir estratégia segura para refresh tokens.
5. Tornar rotação e processamento de notificações atômicos/idempotentes.

### P1

1. Corrigir a constraint de idempotência de `id_local`, se o escopo correto for usuário.
2. Adicionar validação específica para check-in, heartbeat e alteração de senha.
3. Adicionar rate limiting na API.
4. Evitar `DefaultRequestHeaders` mutável no serviço WhatsApp.
5. Mascarar números e dados pessoais nos logs.
6. Adicionar tratamento consistente para violação de unicidade.
7. Definir isolamento seguro para o banco de integração.

### P2

1. Substituir mapeamento por reflection por mapeamento explícito.
2. Implementar APIs realmente assíncronas na Unit of Work ou renomear a abstração.
3. Criar retenção de sessões, heartbeats e eventos.
4. Adicionar métricas, correlation ID e tracing.
5. Criar testes de concorrência, timeout, cancelamento e reprocessamento.
6. Validar configurações tipadas no startup.

## 9. Conclusão

Os cinco projetos restantes foram analisados e os testes disponíveis foram executados com sucesso: 75 unitários e 14 de integração.

A arquitetura é aproveitável e não exige reescrita. Os riscos mais importantes estão em segurança e operação: tokens persistidos em texto puro, refresh vulnerável a corrida, jobs não claramente idempotentes, banco de integração compartilhado e validações de domínio incompletas.

A solução está em um estágio funcional, mas precisa dessas correções antes de ser considerada robusta para produção. A API e o MAUI permanecem documentados em relatórios separados.


---

## Resposta do time — 2026-08-20

**Concordamos com todos os achados críticos.** Issues criadas:

| Achado | Issue | Prioridade |
|---|---|---|
| Refresh token texto puro + corrida | #146 | P0 |
| id_local UNIQUE global (deveria ser por usuário) | #147 | P0 |
| Jobs podem duplicar notificações | já em #104 (corrigido parcialmente) | P0 |
| Banco de integração destrutivo/compartilhado | #139 (testes) | P1 |
| UnitOfWork síncrono com API async falsa | backlog | P2 |
| Mapeamento por reflection frágil | backlog | P2 |

**Observação sobre id_local:** nunca havia sido identificado que a constraint deveria ser por usuário. Issue #147 criada — é uma correção de migration com potencial impacto em produção, precisa de atenção.
