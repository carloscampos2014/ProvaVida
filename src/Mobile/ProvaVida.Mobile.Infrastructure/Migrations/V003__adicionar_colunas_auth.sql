-- Nota: senha_hash já existe na V001 em usuarios — NÃO duplicar aqui

CREATE TABLE IF NOT EXISTS tokens (
    id            TEXT PRIMARY KEY,
    usuario_id    TEXT NOT NULL,
    access_token  TEXT NOT NULL,
    refresh_token TEXT NOT NULL,
    expires_at    TEXT NOT NULL,
    senha_hash    TEXT NOT NULL,
    criado_em     TEXT NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
);
