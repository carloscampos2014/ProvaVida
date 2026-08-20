-- V007: Proteção contra brute force nos endpoints públicos de autenticação

CREATE TABLE IF NOT EXISTS tentativas_login (
    id          UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    ip          VARCHAR(45) NOT NULL,
    endpoint    VARCHAR(100) NOT NULL,
    criado_em   TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_tentativas_login_ip_criado_em
    ON tentativas_login (ip, criado_em DESC);

CREATE TABLE IF NOT EXISTS ips_bloqueados (
    id              UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    ip              VARCHAR(45) NOT NULL,
    motivo          VARCHAR(200) NOT NULL,
    bloqueado_em    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    expira_em       TIMESTAMPTZ NOT NULL,
    liberado_em     TIMESTAMPTZ,
    liberado_por    VARCHAR(100)
);

CREATE INDEX IF NOT EXISTS ix_ips_bloqueados_ip
    ON ips_bloqueados (ip)
    WHERE liberado_em IS NULL;
