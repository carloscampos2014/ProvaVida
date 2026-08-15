-- V004: Adiciona suporte a refresh token na tabela sessoes_login
-- Rotação de refresh token: cada uso invalida o token anterior e emite um novo.

ALTER TABLE sessoes_login
    ADD COLUMN refresh_token         VARCHAR(512)  NULL,
    ADD COLUMN refresh_token_expira_em TIMESTAMP WITH TIME ZONE NULL;

-- Índice para busca rápida por refresh token
CREATE UNIQUE INDEX IF NOT EXISTS ix_sessoes_login_refresh_token
    ON sessoes_login (refresh_token)
    WHERE refresh_token IS NOT NULL;
