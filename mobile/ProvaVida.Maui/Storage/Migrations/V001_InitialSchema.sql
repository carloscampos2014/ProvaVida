-- V001: Schema inicial com tabelas e índices de performance
-- Idempotente — IF NOT EXISTS em todas as instruções

CREATE TABLE IF NOT EXISTS checkins_local (
    id_local                  TEXT    PRIMARY KEY NOT NULL,
    usuario_id                TEXT    NOT NULL DEFAULT '',
    data_hora                 TEXT    NOT NULL DEFAULT '',
    latitude                  REAL,
    longitude                 REAL,
    device_id                 TEXT    NOT NULL DEFAULT '',
    sincronizado              INTEGER NOT NULL DEFAULT 0,
    tentativas_sincronizacao  INTEGER NOT NULL DEFAULT 0
);

CREATE INDEX IF NOT EXISTS ix_checkins_usuario_sincronizado
    ON checkins_local (usuario_id, sincronizado);

CREATE INDEX IF NOT EXISTS ix_checkins_data_hora
    ON checkins_local (usuario_id, data_hora DESC);

CREATE TABLE IF NOT EXISTS heartbeats_local (
    id_local      TEXT    PRIMARY KEY NOT NULL,
    usuario_id    TEXT    NOT NULL DEFAULT '',
    data_hora     TEXT    NOT NULL DEFAULT '',
    sincronizado  INTEGER NOT NULL DEFAULT 0
);

CREATE INDEX IF NOT EXISTS ix_heartbeats_sincronizado
    ON heartbeats_local (sincronizado);
