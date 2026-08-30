CREATE TABLE IF NOT EXISTS usuarios (
    id TEXT PRIMARY KEY NOT NULL,
    nome TEXT NOT NULL,
    email TEXT NOT NULL UNIQUE,
    whatsapp TEXT NOT NULL,
    senha_hash TEXT NOT NULL,
    contato_emergencia_nome TEXT NOT NULL,
    contato_emergencia_email TEXT NOT NULL,
    contato_emergencia_whatsapp TEXT NOT NULL,
    criado_em TEXT NOT NULL,
    atualizado_em TEXT NOT NULL
);
