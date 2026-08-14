-- V002: Tabelas de check-in e heartbeat

CREATE TABLE IF NOT EXISTS checkins (
    id          UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    usuario_id  UUID        NOT NULL REFERENCES usuarios(id) ON DELETE CASCADE,
    id_local    UUID        NOT NULL,
    data_hora   TIMESTAMPTZ NOT NULL,
    latitude    DOUBLE PRECISION,
    longitude   DOUBLE PRECISION,
    device_id   VARCHAR(200) NOT NULL DEFAULT '',
    CONSTRAINT uq_checkins_id_local UNIQUE (id_local)
);

CREATE INDEX IF NOT EXISTS ix_checkins_usuario_id  ON checkins (usuario_id);
CREATE INDEX IF NOT EXISTS ix_checkins_data_hora   ON checkins (usuario_id, data_hora DESC);

CREATE TABLE IF NOT EXISTS heartbeats (
    id          UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    usuario_id  UUID        NOT NULL REFERENCES usuarios(id) ON DELETE CASCADE,
    data_hora   TIMESTAMPTZ NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_heartbeats_usuario_id ON heartbeats (usuario_id);
CREATE INDEX IF NOT EXISTS ix_heartbeats_data_hora  ON heartbeats (usuario_id, data_hora DESC);
