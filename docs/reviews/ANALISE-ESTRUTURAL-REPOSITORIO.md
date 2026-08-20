# Análise estrutural do repositório ProvaVida

## 1. Escopo

Esta análise considera o repositório como um todo:

- solução e organização de diretórios;
- projetos .NET e dependências entre camadas;
- aplicativo MAUI;
- API, domínio e infraestrutura;
- testes;
- CI/CD;
- deploy e operação;
- segurança de arquivos e segredos;
- documentação e governança;
- manutenção e evolução futura.

As análises detalhadas da API, do MAUI e dos projetos não-MAUI permanecem em documentos separados dentro de `.vs`/`docs`.

## 2. Estado atual

A solução possui uma arquitetura funcional dividida em:

- `src/ProvaVida.Domain` — entidades e regras centrais;
- `src/ProvaVida.Application` — casos de uso, interfaces e validações;
- `src/ProvaVida.Infrastructure` — PostgreSQL, Dapper, DbUp, Hangfire, JWT e integrações externas;
- `src/ProvaVida.Api` — composição da API e endpoints HTTP;
- `mobile/ProvaVida.Maui` — aplicativo móvel;
- `tests/` — testes unitários e de integração;
- `deploy/` — Nginx, systemd e setup da VM;
- `.github/workflows` — CI, build APK e deploy da API;
- `docs/` — especificações, arquitetura, QA, LGPD e deploy.

A base arquitetural é aproveitável. O principal problema é a distância entre a estrutura real e a governança/documentação do repositório: há arquivos desatualizados, configurações duplicadas, controles de segurança incompletos e workflows que validam apenas parte do sistema.

## 3. Evidências de validação

Na branch analisada:

- build da API: sucesso;
- testes unitários: 75 aprovados;
- testes de integração: 14 aprovados;
- testes de integração dependem do PostgreSQL local `provavida_dev`;
- build MAUI, publicação APK e deploy real não foram executados localmente nesta análise.

Os testes passando confirmam que os cenários cobertos estão funcionando. Eles não comprovam, por si só, segurança administrativa, concorrência, idempotência em múltiplas instâncias, recuperação de deploy ou prontidão de produção.

## 4. Pontos fortes do repositório

- Separação clara entre domínio, aplicação, infraestrutura, API e mobile.
- Solução `ProvaVida.slnx` organiza corretamente `mobile`, `src` e `tests`.
- Uso de interfaces entre Application e Infrastructure.
- PostgreSQL, Dapper e migrations SQL versionadas.
- Jobs Hangfire e integrações externas isolados em Infrastructure.
- Testes unitários e de integração já existentes.
- CI separado para backend e MAUI.
- Workflow de deploy automatizado por SSH/rsync/systemd.
- Build de APK Release com keystore fornecida por secret.
- Documentação de arquitetura, deploy, QA e LGPD já iniciada.
- `.gitignore` contempla `bin`, `obj`, `.vs`, logs, cobertura e keystores.
- Uso de `TreatWarningsAsErrors` nos projetos principais.
- Há instruções de desenvolvimento e de contribuição para agentes/colaboradores.

## 5. Problemas estruturais prioritários

## P0 — corrigir antes de produção

### 5.1 Corrigir exposição administrativa

A API possui endpoints administrativos sem autorização adequada, conforme detalhado em `.vs/ANALISE-API.md`.

A restrição existente no Nginx não deve ser o único controle. A autorização precisa existir também na API, com policy/role administrativa, porque:

- a API pode ser acessada localmente;
- outra rota/proxy pode ser configurada no futuro;
- testes e ambientes diferentes podem não usar o mesmo Nginx;
- segurança deve estar próxima do recurso protegido.

**Ação:** implementar policy administrativa, aplicar em `AdminController` e criar testes HTTP para 401/403/200.

### 5.2 Remover e rotacionar material sensível

O repositório rastreia:

- `src/ProvaVida.Api/appsettings.Development.json`;
- `src/ProvaVida.Api/logs/*.log`;
- `tests/ProvaVida.IntegrationTests/appsettings.IntegrationTests.json`.

O `.gitignore` não remove arquivos já rastreados. Também existe um arquivo `provavida.keystore` na raiz do workspace, embora esteja ignorado localmente.

Mesmo que os valores sejam de desenvolvimento, senha de banco, chave JWT, logs e keystore não devem ser tratados como arquivos comuns do projeto.

**Ação:**

1. confirmar se a keystore já esteve em algum commit;
2. se esteve, rotacionar/revogar a chave e remover do histórico;
3. remover configurações e logs rastreados do Git;
4. usar User Secrets, variáveis de ambiente e GitHub Secrets;
5. adicionar secret scanning e bloqueio de commits sensíveis.

### 5.3 Definir uma fonte única de verdade para configuração

Há divergências entre:

- `README.md`;
- `docs/Deploy.md`;
- `deploy/scripts/setup-vm.sh`;
- `deploy/systemd/provavida-api.service`;
- `deploy/nginx/provida-api.conf`;
- `.github/workflows/deploy-api.yml`.

Também há diferenças entre os secrets documentados e os secrets realmente escritos pelo workflow. O guia documenta `EMAIL_HOST`, `EMAIL_PORT` e `EMAIL_REMETENTE`, enquanto o workflow deixa parte dessas configurações no `appsettings.json`.

**Ação:** centralizar a lista de configuração em um contrato versionado e validar o ambiente no startup/deploy.

## 6. Arquitetura e organização

### 6.1 Consolidar a governança de agentes e instruções

Existem vários diretórios de configuração de ferramentas de IA, incluindo `.amazonq`, `.claude`, `.codex`, `.github`, `.kiro` e `.trae`, além de `AGENTS.md`, `GEMINI.md` e `QWEN.md`.

Isso pode ser útil para diferentes ferramentas, mas cria risco de:

- regras contraditórias;
- manutenção duplicada;
- instruções obsoletas;
- comportamento diferente conforme a ferramenta usada;
- aumento de ruído na raiz do repositório.

**Ação recomendada:** definir um documento canônico de engenharia, por exemplo `docs/ENGINEERING-STANDARDS.md`, e fazer os arquivos específicos apenas referenciarem esse documento. Manter em cada ferramenta somente o conteúdo necessário para integração.

### 6.2 Adicionar padrões de build na raiz

Não foram encontrados arquivos comuns como:

- `global.json`;
- `Directory.Build.props`;
- `Directory.Build.targets`;
- `Directory.Packages.props`;
- `.editorconfig`;
- `NuGet.config` central;
- `CODEOWNERS`;
- `SECURITY.md`;
- `CONTRIBUTING.md`.

Isso força configurações repetidas em cada `.csproj` e dificulta padronizar SDK, warnings, análise estática e versões de pacotes.

**Ação recomendada:**

- usar `global.json` para fixar o SDK validado;
- usar `Directory.Build.props` para `Nullable`, warnings, análise e linguagem;
- avaliar Central Package Management;
- adicionar `.editorconfig`;
- adicionar `CODEOWNERS`, `SECURITY.md` e `CONTRIBUTING.md`.

### 6.3 Tornar a fronteira das camadas verificável

A arquitetura documentada define a regra:

`Domain ← Application ← Infrastructure ← API`.

Essa regra deve ser verificada automaticamente. Hoje ela depende principalmente da disciplina dos `.csproj`.

**Ação recomendada:** adicionar teste arquitetural ou validação no CI que impeça:

- Domain de referenciar Application/Infrastructure;
- Application de referenciar API/Infrastructure;
- controllers de conter regra de negócio;
- Infrastructure de vazar detalhes para contratos HTTP.

### 6.4 Separar apresentação administrativa

O HTML do painel administrativo está dentro do controller da API. Para o estágio atual pode funcionar, mas crescerá com dificuldade e mistura:

- autorização;
- consulta de dados;
- geração de HTML;
- JavaScript;
- estilos;
- ações de notificação.

**Ação recomendada:** manter a API administrativa em endpoints JSON e mover a interface para um projeto/pasta de apresentação administrativa separada, ou usar uma ferramenta de dashboard protegida.

## 7. Dependências e manutenção

### 7.1 Centralizar versões NuGet

As versões estão espalhadas entre vários `.csproj`. Isso facilita divergência entre API, Infrastructure e testes.

**Ação:** avaliar `Directory.Packages.props`, mantendo versões comuns de .NET, Npgsql, Dapper, xUnit, FluentAssertions e ferramentas de teste em um único local.

### 7.2 Tratar supressões de auditoria como exceções temporárias

Existem supressões para avisos de MailKit/MimeKit e outras vulnerabilidades. Uma supressão sem prazo de revisão pode virar risco permanente.

**Ação:** para cada supressão, registrar:

- advisory/CVE;
- pacote afetado;
- motivo;
- versão corrigida esperada;
- responsável;
- data de revisão.

Adicionar ao CI uma tarefa de auditoria de pacotes e falhar quando surgir vulnerabilidade não aceita.

### 7.3 Remover dependências e referências obsoletas

A documentação menciona EF Core em pontos onde a implementação atual usa Dapper/DbUp. Também há dependências que devem ser revisadas para confirmar se são realmente necessárias.

**Ação:** executar periodicamente `dotnet list package --outdated` e revisar dependências não utilizadas, mantendo a decisão documentada.

## 8. CI/CD

### 8.1 O CI não cobre o sistema inteiro

O workflow `ci.yml` executa build da API e testes unitários. Ele não executa os testes de integração, que são essenciais para o PostgreSQL, WebApplicationFactory e fluxos de inatividade.

Também não executa:

- auditoria NuGet;
- análise de formatação;
- análise estática;
- verificação de segredos;
- testes de contrato;
- validação de migrations;
- validação dos scripts de deploy.

**Ação:** adicionar jobs separados para:

1. restore/build;
2. unitários;
3. PostgreSQL de teste + integração;
4. `dotnet format --verify-no-changes`;
5. auditoria de dependências;
6. secret scanning;
7. validação de YAML/shell/PowerShell;
8. relatório de cobertura.

### 8.2 Deploy não executa testes de integração

`deploy-api.yml` executa testes unitários antes do publish, mas não os testes de integração.

**Ação:** exigir integração verde antes do deploy ou separar pipeline de validação com proteção de ambiente.

### 8.3 Deploy direto no `master`

O deploy é disparado por push em `master`. Isso pode ser aceitável para um projeto pequeno, mas é arriscado sem:

- branch protection;
- pull request obrigatório;
- revisão aprovada;
- checks obrigatórios;
- aprovação de ambiente de produção;
- possibilidade de rollback automatizado.

**Ação:** proteger `master`, exigir CI verde e usar environment `production` com aprovação quando a API entrar em uso real.

### 8.4 Actions não estão fixadas por SHA

Os workflows usam actions como `actions/checkout@v4` e `actions/setup-dotnet@v4`. Tags major são práticas, mas não oferecem reprodutibilidade máxima contra alteração da referência.

**Ação:** em ambiente de produção, fixar actions por commit SHA e atualizar de forma controlada.

### 8.5 Falta de rollback operacional real

O deploy interrompe o serviço, substitui os arquivos e inicia novamente. Se o novo build falhar depois da cópia, o serviço pode ficar indisponível.

**Ação recomendada:**

- publicar em diretório versionado;
- validar o pacote antes da troca;
- usar symlink `current`/`releases`;
- manter a última versão funcional;
- fazer health check antes de concluir;
- reverter automaticamente se o health check falhar.

### 8.6 Migrations acopladas ao startup

A API aplica migrations no startup. Isso simplifica o fluxo, mas aumenta o risco de indisponibilidade durante deploy e de corrida entre instâncias.

**Ação:** considerar um job explícito de migration antes do rollout e impedir que a aplicação receba tráfego antes da confirmação do schema.

## 9. Segurança do repositório e da operação

### 9.1 Arquivos rastreados contradizem a política do `.gitignore`

O `.gitignore` declara que configurações de ambiente e logs não devem ser versionados, mas arquivos correspondentes aparecem no índice Git.

Isso indica que a política existe, mas não é aplicada retroativamente.

**Ação:** executar revisão de histórico, remover arquivos rastreados, rotacionar qualquer credencial e adicionar verificação automatizada.

### 9.2 Segredos em argumentos de build/deploy

O workflow passa senhas Android como parâmetros de `dotnet publish`. O GitHub mascara secrets em logs, mas argumentos de processo podem aparecer em diagnósticos ou ambientes intermediários.

**Ação:** usar arquivos temporários protegidos, propriedades de ambiente ou mecanismo de assinatura recomendado pelo SDK, sempre removendo os artefatos no `always()`.

### 9.3 Hardening incompleto do systemd

O serviço usa `NoNewPrivileges`, `PrivateTmp` e `ProtectSystem`, o que é positivo. Pode evoluir com:

- `ProtectHome=true`;
- `PrivateDevices=true`;
- `ProtectKernelTunables=true`;
- `ProtectControlGroups=true`;
- `RestrictAddressFamilies` compatível com PostgreSQL/SMTP/HTTP;
- limites de memória/CPU;
- usuário sem permissões além do diretório publicado.

Cada opção deve ser validada contra o comportamento real da aplicação.

### 9.4 Nginx e script de setup podem divergir

O arquivo versionado do Nginx contém restrição para `/admin`, mas o script inicial de setup cria uma configuração simplificada sem essa restrição quando o arquivo ainda não existe.

Isso pode gerar ambientes diferentes dependendo da ordem de execução.

**Ação:** manter o server block em um único template oficial e fazer o script instalar exatamente o arquivo versionado.

## 10. Documentação

### 10.1 Corrigir informações desatualizadas

Foram encontradas inconsistências concretas:

- `README.md` cita .NET 9 nos pré-requisitos, enquanto a solução usa .NET 10;
- `README.md` descreve o MAUI como “a implementar”, embora já exista uma implementação significativa;
- README cita Swagger, enquanto a API atual usa Scalar/OpenAPI;
- documentação de arquitetura menciona proxy para porta 5000, enquanto deploy usa 5001;
- arquitetura menciona EF Core migrations, enquanto o código usa DbUp;
- documentação de segurança afirma que todos os endpoints, exceto autenticação, estão protegidos, mas o AdminController precisa ser revisado;
- QA ainda possui muitos cenários pendentes, apesar de README indicar fases concluídas;
- cronograma ainda descreve publicação em lojas, enquanto o fluxo atual gera APK direto.

**Ação:** definir uma revisão de documentação como etapa obrigatória de cada release.

### 10.2 Criar documentação operacional mínima

Adicionar documentos curtos e objetivos:

- `docs/Local-Development.md`;
- `docs/Testing.md`;
- `docs/Configuration.md`;
- `docs/Release.md`;
- `docs/Incident-Response.md`;
- `docs/Architecture-Decision-Records/`;
- `docs/API-Contract.md` ou OpenAPI publicado.

### 10.3 Mover análises canônicas para `docs`

`.vs` é ignorado pelo Git. Os relatórios salvos ali são úteis localmente, mas não são compartilhados no repositório nem preservados no histórico.

**Recomendação:** manter o documento solicitado em `.vs`, mas copiar a versão aprovada para `docs/reviews/` se a intenção for compartilhar a análise com a equipe.

## 11. Testes e qualidade

### Estado atual

- 75 testes unitários aprovados.
- 14 testes de integração aprovados.
- Integração depende de PostgreSQL local.
- Jobs Hangfire reais não são executados nos testes de integração.
- Não há evidência de cobertura publicada no CI.

### Melhorias recomendadas

1. Adicionar testes de autorização administrativa.
2. Adicionar testes de concorrência de refresh token.
3. Adicionar testes de idempotência de jobs.
4. Adicionar testes de falhas parciais de e-mail/WhatsApp/SMS/voz.
5. Adicionar testes de migrations em banco limpo.
6. Isolar banco por execução.
7. Adicionar testes de contrato da API.
8. Executar testes MAUI de ViewModels/serviços sem depender da plataforma.
9. Adicionar smoke test pós-deploy.
10. Publicar cobertura por projeto e estabelecer um baseline, sem transformar cobertura em único indicador de qualidade.

## 12. Estrutura alvo recomendada

Uma evolução razoável, sem reescrever o projeto, seria:

```text
/
  .github/
	CODEOWNERS
	workflows/
	pull_request_template.md
  deploy/
	nginx/
	systemd/
	scripts/
  docs/
	architecture/
	adr/
	operations/
	security/
	testing/
	reviews/
  mobile/
	ProvaVida.Maui/
  src/
	ProvaVida.Domain/
	ProvaVida.Application/
	ProvaVida.Infrastructure/
	ProvaVida.Api/
  tests/
	ProvaVida.Domain.Tests/          (se houver lógica própria suficiente)
	ProvaVida.Application.Tests/
	ProvaVida.Infrastructure.Tests/ (repositórios/migrations)
	ProvaVida.IntegrationTests/
	ProvaVida.ArchitectureTests/
  Directory.Build.props
  Directory.Build.targets
  Directory.Packages.props
  .editorconfig
  global.json
  README.md
  SECURITY.md
  CONTRIBUTING.md
```

Essa é uma direção, não uma exigência de criação imediata de todos os projetos.

## 13. Roadmap de implementação

### Fase 1 — segurança e controle do repositório

1. Proteger `AdminController` e adicionar testes.
2. Revisar/rotacionar secrets, appsettings, logs e keystore.
3. Remover arquivos sensíveis rastreados do Git e histórico quando necessário.
4. Proteger branch `master`.
5. Adicionar secret scanning e auditoria NuGet ao CI.
6. Corrigir configuração Nginx/script para que sejam idênticos.

### Fase 2 — pipeline confiável

1. Executar integração no CI com PostgreSQL de teste.
2. Adicionar validação de formatação e análise estática.
3. Fixar versões do SDK e pacotes centrais.
4. Adicionar environment de produção e aprovação.
5. Implementar deploy versionado e rollback.
6. Adicionar smoke test pós-deploy.

### Fase 3 — consistência técnica

1. Centralizar configuração e documentação.
2. Corrigir divergências de portas, .NET, Swagger/Scalar e DbUp/EF.
3. Adicionar testes arquiteturais.
4. Melhorar validações de domínio.
5. Tornar refresh e jobs idempotentes.
6. Revisar retenção e observabilidade.

### Fase 4 — evolução de produto

1. Completar QA manual pendente em ambiente de homologação.
2. Definir política de privacidade e termos revisados juridicamente.
3. Formalizar SLOs de disponibilidade, latência e entrega de notificações.
4. Adicionar monitoramento e alertas de negócio.
5. Planejar escala além da VM única quando os requisitos justificarem.

## 14. Veredito

A estrutura do repositório é boa o suficiente para continuar evoluindo, mas ainda está muito dependente de conhecimento implícito e de operação manual. O maior ganho não virá de criar mais camadas; virá de tornar as regras existentes verificáveis e consistentes.

As melhorias mais importantes são:

1. segurança real no código, não apenas no Nginx;
2. remoção efetiva de arquivos sensíveis do histórico e do workspace compartilhável;
3. CI cobrindo integração, segurança e qualidade;
4. deploy com rollback e configuração validada;
5. documentação alinhada ao código atual;
6. centralização de versões e padrões;
7. testes para concorrência, jobs e falhas externas.

Depois dessas mudanças, o projeto terá uma estrutura mais previsível, auditável e segura para colaboração e produção, sem exigir uma reescrita completa da solução.


---

## Resposta do time — 2026-08-20

**Análise estrutural excelente e abrangente.** Issues e ações:

| Achado | Issue/Ação | Prioridade |
|---|---|---|
| AdminController exposto | #145 | P0 |
| Segredos rastreados | já no .gitignore, verificar histórico | P0 |
| CI não cobre integração | backlog | P1 |
| health checks reais | #149 | P1 |
| Documentação desatualizada | #151 | P2 |
| Directory.Build.props / global.json | #150 | P3 |
| Múltiplos dirs de ferramentas AI | decisão intencional, baixa prioridade | P3 |

**Discordamos em parte:** a proliferação de diretórios de ferramentas AI (`.kiro`, `.claude`, `.trae`, etc.) é intencional — cada ferramenta tem sua própria convenção de leitura. Um documento canônico em `docs/ENGINEERING-STANDARDS.md` que seja referenciado por todos é uma boa ideia e será considerado.
