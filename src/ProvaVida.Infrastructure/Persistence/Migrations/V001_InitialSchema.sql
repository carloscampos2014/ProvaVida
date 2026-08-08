-- V001: Schema inicial ProvaVida
-- Tabelas: usuarios, sessoes_login

CREATE TABLE IF NOT EXISTS usuarios (
    id                          UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    nome                        VARCHAR(200) NOT NULL,
    email                       VARCHAR(300) NOT NULL,
    whatsapp                    VARCHAR(20)  NOT NULL,
    senha_hash                  VARCHAR(500) NOT NULL,
    ativo                       BOOLEAN      NOT NULL DEFAULT TRUE,
    contato_emergencia_nome     VARCHAR(200) NOT NULL,
    contato_emergencia_email    VARCHAR(300) NOT NULL,
    contato_emergencia_whatsapp VARCHAR(20)  NOT NULL,
    criado_em                   TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    atualizado_em               TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_usuarios_email ON usuarios (LOWER(email));

CREATE TABLE IF NOT EXISTS sessoes_login (
    id          UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    usuario_id  UUID         NOT NULL REFERENCES usuarios(id) ON DELETE CASCADE,
    token       VARCHAR(2000) NOT NULL,
    criado_em   TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    expira_em   TIMESTAMPTZ  NOT NULL,
    ativo       BOOLEAN      NOT NULL DEFAULT TRUE
);

CREATE INDEX IF NOT EXISTS ix_sessoes_token      ON sessoes_login (token);
CREATE INDEX IF NOT EXISTS ix_sessoes_usuario_id ON sessoes_login (usuario_id);
