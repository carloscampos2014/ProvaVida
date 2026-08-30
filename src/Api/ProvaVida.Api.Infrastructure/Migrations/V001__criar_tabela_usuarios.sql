CREATE TABLE IF NOT EXISTS usuarios (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    nome VARCHAR(200) NOT NULL,
    email VARCHAR(200) NOT NULL UNIQUE,
    whatsapp VARCHAR(20) NOT NULL,
    senha_hash VARCHAR(64) NOT NULL,
    contato_emergencia_nome VARCHAR(200) NOT NULL,
    contato_emergencia_email VARCHAR(200) NOT NULL,
    contato_emergencia_whatsapp VARCHAR(20) NOT NULL,
    criado_em TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    atualizado_em TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);
