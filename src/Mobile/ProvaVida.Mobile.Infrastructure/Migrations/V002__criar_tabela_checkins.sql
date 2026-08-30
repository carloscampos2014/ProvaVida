CREATE TABLE IF NOT EXISTS checkins (
    id TEXT PRIMARY KEY NOT NULL,
    usuario_id TEXT NOT NULL,
    data TEXT NOT NULL,
    latitude REAL NOT NULL,
    longitude REAL NOT NULL,
    identificacao_aparelho TEXT NOT NULL,
    sincronizado INTEGER NOT NULL DEFAULT 0,
    criado_em TEXT NOT NULL,
    FOREIGN KEY (usuario_id) REFERENCES usuarios(id) ON DELETE CASCADE,
    UNIQUE (usuario_id, data)
);
