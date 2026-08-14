-- V003: Tabela de notificações de emergência

CREATE TABLE IF NOT EXISTS notificacoes_emergencia (
    id                UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    usuario_id        UUID        NOT NULL REFERENCES usuarios(id) ON DELETE CASCADE,
    status            VARCHAR(30) NOT NULL,
    canal             VARCHAR(50) NOT NULL DEFAULT 'nenhum',
    data_disparo      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    janela_expira_em  TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_notif_usuario_id    ON notificacoes_emergencia (usuario_id);
CREATE INDEX IF NOT EXISTS ix_notif_status        ON notificacoes_emergencia (status);
CREATE INDEX IF NOT EXISTS ix_notif_janela_expira ON notificacoes_emergencia (janela_expira_em)
    WHERE status = 'aguardando_resposta';
