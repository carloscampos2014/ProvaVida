CREATE TABLE IF NOT EXISTS heartbeats (
    id TEXT PRIMARY KEY NOT NULL,
    usuario_id TEXT NOT NULL,
    recebido_em TEXT NOT NULL,
    criado_em TEXT NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (usuario_id) REFERENCES usuarios(id) ON DELETE CASCADE
);