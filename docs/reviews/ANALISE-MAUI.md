# Análise técnica do projeto ProvaVida.Maui

**Projeto analisado:** `mobile/ProvaVida.Maui`

**Target framework:** .NET 10 MAUI

**Ambiente informado:** Visual Studio Community 2026 — 18.9.0

**Branch:** `fix/warnings-definitivo`

**Data da análise:** 2026-02-01

## 1. Resumo executivo

O projeto possui uma base organizada e uma arquitetura inicial adequada para um aplicativo .NET MAUI: há separação entre Pages, ViewModels, Services, Storage e Models; uso de injeção de dependência; persistência local com SQLite; armazenamento de tokens com SecureStorage; além de integração com widgets, tile, notificações e atalhos Android.

A compilação do projeto foi validada com sucesso.

Apesar disso, o aplicativo ainda apresenta riscos relevantes para produção, principalmente em sincronização offline, concorrência, ciclo de vida das páginas, autenticação, uso de dados locais por usuário e tratamento de datas. A avaliação geral é de um protótipo avançado ou MVP funcional, mas não de uma aplicação totalmente endurecida para cenários reais de produção.

## 2. Avaliação por área

| Área | Avaliação |
|---|---:|
| Organização | 7/10 |
| Segurança | 5/10 |
| Robustez offline | 4/10 |
| Concorrência e ciclo de vida | 4/10 |
| Integração Android | 5/10 |
| Testabilidade | 3/10 |
| Prontidão para produção | 5/10 |

## 3. Pontos positivos

- Estrutura de diretórios compreensível.
- Separação entre Pages, ViewModels, Services, Storage e Models.
- Uso de interfaces para serviços e armazenamento.
- Nullable reference types habilitado.
- Uso de `SecureStorage` para tokens JWT e refresh tokens.
- Uso de SQLite para suporte offline.
- Inicialização do banco protegida por `SemaphoreSlim`.
- Existência de mecanismos de logout e exclusão de dados locais.
- Preocupação explícita com LGPD.
- Integração com widgets, tile, notificações e atalhos Android.
- Definição explícita das versões mínimas das plataformas.
- Compilação atual validada com sucesso.

## 4. Problemas críticos

### 4.1 Sincronização pode perder registros

Em `Services/CheckInService.cs`, falhas HTTP aparentemente são convertidas em `false`. Em `Services/SyncService.cs`, esse resultado pode ser interpretado como conclusão da operação e o registro local pode ser marcado como sincronizado.

Isso pode causar perda silenciosa de dados em situações como:

- resposta HTTP 401;
- erro HTTP 500;
- falha de rede;
- token expirado;
- indisponibilidade temporária da API.

**Recomendação:** modelar explicitamente os resultados da sincronização, diferenciando sucesso, falha temporária, falha permanente e sessão expirada. O registro só deve ser marcado como sincronizado após confirmação do servidor.

### 4.2 Uso excessivo de `Task.Run` e fire-and-forget

Há operações iniciadas sem serem aguardadas em `CheckInViewModel`, `PerfilViewModel`, `LoadingPage`, `CheckInPage` e integrações Android.

Isso pode produzir:

- exceções não observadas;
- atualização da UI fora da thread correta;
- tarefas continuando após a destruição da página;
- múltiplas sincronizações simultâneas;
- navegação antes da conclusão da inicialização;
- comportamento inconsistente no retorno à página.

**Recomendação:** usar métodos assíncronos aguardados, `CancellationToken`, proteção contra reentrada e comandos assíncronos. Operações de rede e SQLite já possuem APIs assíncronas e não precisam ser colocadas indiscriminadamente em `Task.Run`.

### 4.3 Concorrência no refresh do token

O refresh do JWT não parece estar protegido contra chamadas simultâneas. Heartbeat, sincronização e carregamento de dados podem tentar renovar o token ao mesmo tempo.

Dependendo do comportamento do backend, isso pode invalidar refresh tokens ou sobrescrever tokens válidos.

**Recomendação:** implementar uma renovação única utilizando `SemaphoreSlim`, `AsyncLazy` ou uma fila de renovação. Tokens definitivamente inválidos também devem ser removidos.

### 4.4 Uso compartilhado de `DefaultRequestHeaders.Authorization`

O mesmo `HttpClient` é compartilhado por vários serviços e o header de autorização é alterado diretamente. Isso é arriscado porque requisições simultâneas podem alterar o header umas das outras.

**Recomendação:** adicionar o token em cada `HttpRequestMessage` ou utilizar um `DelegatingHandler` responsável pela autenticação.

### 4.5 Dados locais não claramente isolados por usuário

A exclusão de dados no `LocalDatabase` e algumas consultas utilizadas por widgets e receivers parecem operar sobre todos os registros locais, sem filtrar consistentemente pelo usuário atual.

Isso pode causar exposição de dados ou comportamento incorreto quando mais de uma conta for usada no mesmo aparelho.

**Recomendação:** adicionar `UsuarioId` aos registros locais e aplicar o filtro em todas as operações de leitura, sincronização, widgets, receivers e exclusão.

## 5. Datas e fusos horários

Há mistura entre `DateTime.UtcNow`, `DateTime.Today` e datas locais em consultas, widgets, tile e indicadores semanais.

Isso pode gerar erros em check-ins próximos à meia-noite ou em aparelhos cujo fuso horário não coincida com UTC.

**Recomendação:**

- armazenar instantes em UTC;
- converter para o fuso do usuário somente na apresentação;
- definir formalmente o conceito de “dia do check-in”;
- considerar `DateTimeOffset`;
- centralizar regras de data e hora em um serviço.

## 6. Autenticação e armazenamento

### Aspectos positivos

- Tokens são armazenados por meio de `SecureStorage`.
- Existe abstração `ITokenStorage`.
- O logout limpa dados locais.
- A exclusão de conta demonstra preocupação com LGPD.

### Riscos

- Claims do JWT são lidos localmente sem validação de assinatura.
- A validade da sessão depende de metadados armazenados separadamente.
- Tokens podem permanecer armazenados após falha definitiva de refresh.
- O fluxo de autenticação está distribuído entre AuthService, ViewModels, LoadingPage e componentes Android.
- Dados como nome, e-mail e contatos de emergência são armazenados em `Preferences` sem proteção equivalente ao token.

Ler claims para fins puramente visuais pode ser aceitável, mas esses valores não devem ser tratados como confiáveis para autorização ou decisões de segurança.

## 7. Banco local

### Pontos positivos

- Uso de SQLite assíncrono.
- Inicialização protegida contra concorrência.
- Serviço centralizado para o banco.
- Existência de limpeza de dados locais.

### Pontos a melhorar

- Não há estratégia clara de migração do banco.
- Não há índices evidentes para as consultas mais frequentes.
- Não há política de retenção de dados antigos.
- Operações relacionadas à sincronização não parecem estar agrupadas em transações.
- Há risco de concorrência em operações de leitura, atualização e sincronização.
- Heartbeats não parecem estar claramente vinculados ao usuário.
- O banco pode crescer indefinidamente.

## 8. Integração Android

### Receivers e alarmes

`AvisoInatividadeReceiver` realiza operações síncronas de SQLite dentro de `BroadcastReceiver.OnReceive`. Isso pode exceder a janela de execução permitida pelo Android.

Também é necessário revisar o reagendamento de alarmes após:

- reinicialização do dispositivo;
- mudança de fuso horário;
- mudança de horário;
- atualização do aplicativo.

### Notificações

O serviço de notificações trata algumas falhas de forma silenciosa. Também deve considerar corretamente:

- permissão de notificações no Android 13+;
- permissão para alarmes exatos;
- canais de notificação;
- ícone válido de notificação;
- restrições de execução em segundo plano de fabricantes.

### Widgets e tile

Widgets e tile parecem usar a existência de um token como indicação de sessão válida. Um token expirado ainda pode fazer o aplicativo parecer autenticado.

Também há chamadas bloqueantes como `.GetAwaiter().GetResult()` em integração Android, o que pode provocar bloqueios ou deadlocks.

## 9. ViewModels e ciclo de vida

A separação entre ViewModels e páginas é positiva, mas ainda há acoplamento entre ciclo de vida e execução de operações.

Problemas observados:

- comandos podem permanecer habilitados durante carregamentos;
- `ChangeCanExecute()` não é atualizado consistentemente;
- login, cadastro ou check-in podem ser disparados repetidamente;
- exceções são capturadas genericamente ou ignoradas;
- operações iniciadas no construtor do ViewModel são difíceis de controlar;
- operações pendentes não são canceladas ao sair da página;
- `OnAppearing` pode iniciar novamente operações já em andamento.

**Recomendação:** adotar uma estratégia única para comandos assíncronos, cancelamento, estado de carregamento e tratamento de erros.

## 10. Validação de entrada

O cadastro possui validações básicas, incluindo campos obrigatórios, tamanho mínimo de senha e aceite dos termos.

Ainda seria recomendável validar:

- formato de e-mail;
- telefone e WhatsApp;
- tamanho máximo dos campos;
- política de senha;
- caracteres inválidos;
- consistência dos dados do contato de emergência.

Essas validações melhoram a experiência, mas não substituem a validação no backend.

## 11. Testes

Não foram identificados testes específicos do projeto MAUI para:

- `AuthService`;
- `SyncService`;
- `LocalDatabase`;
- expiração e refresh de token;
- falhas de rede;
- sincronização duplicada;
- widgets;
- receivers;
- notificações;
- armazenamento seguro;
- conversão de fusos horários.

Essa é uma fraqueza importante porque os principais riscos aparecem em situações de concorrência, offline e ciclo de vida, difíceis de validar manualmente.

### Testes prioritários

1. Falha de rede durante sincronização.
2. Resposta 401 durante sincronização.
3. Dois refreshes de token simultâneos.
4. Check-in próximo da meia-noite.
5. Troca de usuário no mesmo dispositivo.
6. Exclusão da conta com sincronização pendente.
7. Reinicialização do Android.
8. Permissão de notificação negada.
9. Widget sem token ou com token expirado.
10. Reentrada da página de check-in.

## 12. Plano recomendado de correção

### Prioridade 1 — antes de produção

1. Corrigir o tratamento de sucesso e falha do `SyncService`.
2. Impedir sincronizações simultâneas.
3. Implementar refresh de token protegido contra concorrência.
4. Remover a alteração de headers compartilhados do `HttpClient`.
5. Corrigir filtros por usuário no SQLite.
6. Corrigir conversões UTC/local.
7. Remover `Task.Run` e fire-and-forget dos fluxos críticos.
8. Cancelar operações ao sair das páginas.

### Prioridade 2

1. Criar testes de autenticação e sincronização.
2. Melhorar o tratamento de falhas de rede.
3. Revisar o armazenamento de dados pessoais.
4. Criar migrações e índices no banco.
5. Revisar receivers, alarmes e widgets após reinicialização.
6. Tratar permissões modernas do Android.

### Prioridade 3

1. Melhorar validações dos formulários.
2. Centralizar mensagens de erro.
3. Melhorar atualização do estado dos comandos.
4. Remover assets padrão não utilizados.
5. Centralizar a configuração da API por ambiente.
6. Adicionar logs estruturados para diagnóstico.

## 13. Conclusão

O projeto tem uma base funcional e uma arquitetura inicial aceitável. O maior problema não está na organização visual do código, mas na robustez em situações reais: perda de conexão, token expirado, execução em segundo plano, troca de usuário, virada do dia e múltiplas operações simultâneas.

A correção do fluxo de sincronização, da autenticação, do ciclo de vida e do isolamento dos dados locais deve ser tratada como prioridade. Depois dessas correções, o aplicativo estará significativamente mais confiável e preparado para evolução em produção.


---

## Resposta do time — 2026-08-20

**Análise correta e alinhada com os problemas que estamos enfrentando.** Issues criadas:

| Achado | Issue | Prioridade |
|---|---|---|
| SyncService pode perder registros | #128 | P1 |
| Task.Run excessivo | #129 | P1 |
| Datas UTC/local misturadas | #133 | P1 |
| Dados locais não isolados por usuário | #132 | P1 |
| HttpClient headers compartilhados | #131 | P1 |
| Refresh token sem proteção concorrência | #130 | P1 |
| Receivers Android síncronos no OnReceive | #134 | P2 |
| Alarmes não reagendados após reboot | #135 | P2 |
| Testes faltantes para serviços críticos | #139 | P2 |
| Widgets e tile com token expirado | #137 | P2 |

**Observação:** a migração para Plugin.LocalNotification (PR #143) resolve o problema dos receivers Android síncronos e dos alarmes não funcionando no Android 15/HyperOS 2, que foi o problema mais urgente identificado em testes reais.
