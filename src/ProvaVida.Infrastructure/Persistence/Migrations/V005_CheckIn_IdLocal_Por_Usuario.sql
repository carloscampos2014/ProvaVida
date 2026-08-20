-- V005: Corrigir constraint de idempotência do check-in para ser por usuário
-- A constraint anterior era global (uq_checkins_id_local), o que permitia que
-- um id_local gerado por um usuário descartasse silenciosamente o check-in de outro.
-- A constraint correta é UNIQUE(usuario_id, id_local).

-- Remove a constraint global antiga
ALTER TABLE checkins DROP CONSTRAINT IF EXISTS uq_checkins_id_local;

-- Adiciona a constraint composta correta
ALTER TABLE checkins
    ADD CONSTRAINT uq_checkins_usuario_id_local UNIQUE (usuario_id, id_local);
