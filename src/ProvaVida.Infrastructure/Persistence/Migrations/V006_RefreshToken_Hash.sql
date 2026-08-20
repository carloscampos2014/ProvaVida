-- V006: Armazenar hash SHA-256 do refresh token em vez do valor original
-- Garante que o vazamento do banco não expõe tokens reutilizáveis.
-- Todos os refresh tokens existentes são invalidados (ativo=false)
-- pois o hash seria incorreto para os valores já armazenados.

-- Invalida todas as sessões ativas com refresh token para forçar novo login
UPDATE sessoes_login
SET ativo = FALSE
WHERE refresh_token IS NOT NULL AND ativo = TRUE;

-- Renomeia a coluna para deixar claro que armazena o hash
ALTER TABLE sessoes_login
    RENAME COLUMN refresh_token TO refresh_token_hash;

-- Índice para busca eficiente por hash
CREATE INDEX IF NOT EXISTS ix_sessoes_login_refresh_token_hash
    ON sessoes_login (refresh_token_hash)
    WHERE refresh_token_hash IS NOT NULL;
