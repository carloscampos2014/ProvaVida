# Requirements Document

## Introduction

A Fase 2 implementa autenticação completa no app Mobile MAUI do ProvaVida. O app gera o hash SHA-256 da senha localmente antes de enviar à API, persiste sessão e tokens no SQLite, renova tokens silenciosamente e segue os fluxos documentados em `docs/Fluxo-Login.md`, `docs/Fluxo-Cadastrar-Conta.md`, `docs/Fluxo-Inicialização.md` e `docs/Fluxo-Logoff.md`.

## Requirements

### RF-201 — Geração de Hash SHA-256 no Dispositivo

**User Story:** Como usuário do app, quero que minha senha nunca trafegue pela rede em texto puro, para que minha conta esteja protegida mesmo em redes inseguras.

#### Acceptance Criteria

1. WHEN o usuário informa a senha THEN o app SHALL gerar o hash SHA-256 antes de qualquer chamada à API
2. WHEN o hash é gerado THEN a senha em texto puro SHALL ser descartada e nunca persistida
3. WHEN o login é bem-sucedido THEN o hash SHALL ser salvo no SQLite local para uso em renovação silenciosa de token

---

### RF-202 — Tela de Cadastro

**User Story:** Como novo usuário, quero me cadastrar pelo app informando meus dados e os do meu contato de emergência, para que eu possa usar o ProvaVida.

#### Acceptance Criteria

1. WHEN o usuário acessa a tela de cadastro THEN o app SHALL exibir campos: nome, e-mail, senha, confirmação de senha, WhatsApp, nome do contato de emergência, e-mail do contato e WhatsApp do contato
2. WHEN o usuário tenta cadastrar sem internet THEN o app SHALL exibir "Não é possível efetuar cadastro sem acesso à internet."
3. WHEN campos obrigatórios estão em branco THEN o app SHALL exibir "Preencha todos os campos obrigatórios."
4. WHEN o cadastro é enviado THEN o app SHALL gerar hash SHA-256 da senha antes de chamar `POST /auth/register`
5. WHEN o servidor retorna sucesso THEN o app SHALL exibir "Cadastro realizado com sucesso!" e navegar para a tela de Login
6. WHEN o servidor retorna erro THEN o app SHALL exibir a mensagem de erro recebida

---

### RF-203 — Tela de Login

**User Story:** Como usuário cadastrado, quero fazer login pelo app, para acessar a tela de Check-in.

#### Acceptance Criteria

1. WHEN o usuário acessa a tela de login THEN o app SHALL exibir campos: e-mail e senha
2. WHEN o usuário tenta fazer login sem internet THEN o app SHALL exibir erro e encerrar o app
3. WHEN o login é enviado THEN o app SHALL gerar hash SHA-256 da senha antes de chamar `POST /auth/login`
4. WHEN o servidor retorna sucesso THEN o app SHALL persistir dados do usuário, access token, refresh token e hash no SQLite e navegar para a tela de Check-in
5. WHEN o servidor retorna erro THEN o app SHALL exibir "Login ou senha inválidos." e manter na tela

---

### RF-204 — Gerenciamento de Sessão

**User Story:** Como usuário, quero que o app lembre minha sessão entre aberturas, para não precisar fazer login toda vez.

#### Acceptance Criteria

1. WHEN existe registro de `Usuario` no SQLite THEN o app SHALL considerar o usuário como logado
2. WHEN `ClearSessionAsync` é chamado THEN o app SHALL excluir dados do usuário e check-ins do SQLite
3. WHEN `GetCurrentUserAsync` é chamado com usuário logado THEN o app SHALL retornar os dados do usuário do SQLite

---

### RF-205 — Renovação Silenciosa de Token

**User Story:** Como usuário, quero que o app renove meu token automaticamente em segundo plano, para não ser interrompido durante o uso.

#### Acceptance Criteria

1. WHEN o access token está expirado THEN o app SHALL tentar renovar via `POST /auth/refresh` sem interromper o usuário
2. WHEN o refresh token também é inválido THEN o app SHALL tentar login silencioso usando o hash salvo no SQLite
3. WHEN o login silencioso falha THEN o app SHALL limpar a sessão e redirecionar para a tela de Login
4. WHEN a renovação é bem-sucedida THEN o app SHALL salvar os novos tokens no SQLite

---

### RF-206 — Fluxo de Inicialização

**User Story:** Como usuário, quero que o app inicie rapidamente e me leve para a tela certa, para ter uma boa experiência desde a abertura.

#### Acceptance Criteria

1. WHEN o app inicia THEN o app SHALL verificar e executar migrations DbUp no SQLite
2. WHEN a criação do banco falha THEN o app SHALL exibir "Não foi possível criar/atualizar o banco de dados local." e encerrar
3. WHEN o usuário está logado (registro no SQLite) THEN o app SHALL navegar diretamente para a tela de Check-in
4. WHEN o usuário não está logado e há internet THEN o app SHALL navegar para a tela de Login
5. WHEN o usuário não está logado e não há internet THEN o app SHALL exibir "Não é possível efetuar login/cadastro sem acesso à internet." e encerrar

---

### RF-207 — Logoff

**User Story:** Como usuário, quero poder sair da minha conta, para proteger meus dados em dispositivos compartilhados.

#### Acceptance Criteria

1. WHEN o usuário faz logoff THEN o app SHALL chamar `POST /auth/logout` na API para invalidar o refresh token
2. WHEN o logoff é processado THEN o app SHALL excluir dados de check-ins do SQLite
3. WHEN o logoff é processado THEN o app SHALL excluir dados do usuário do SQLite
4. WHEN a limpeza é concluída THEN o app SHALL navegar para a tela de Login

---

## Glossary

| Termo | Definição |
|---|---|
| Hash SHA-256 | Representação criptográfica unidirecional da senha, gerada no dispositivo antes de qualquer transmissão |
| Access Token | JWT de curta duração utilizado para autenticar requisições à API |
| Refresh Token | Token de longa duração usado para renovar o access token sem nova senha |
| Login Silencioso | Renovação automática de sessão usando o hash salvo no SQLite, sem interação do usuário |
| Sessão | Estado de "logado" determinado pela existência de registro de `Usuario` no SQLite local |
| SQLite local | Banco de dados SQLite no dispositivo, fonte de verdade do estado de autenticação |
