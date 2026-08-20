-- V002: Normaliza data_hora para incluir sufixo Z (UTC explícito)
-- Necessário para compatibilidade com DateTimeOffset no sqlite-net-pcl
-- Idempotente — WHERE garante que registros já normalizados não são alterados

UPDATE checkins_local
SET data_hora = data_hora || 'Z'
WHERE data_hora NOT LIKE '%+%'
  AND data_hora NOT LIKE '%Z%'
  AND data_hora != '';

UPDATE heartbeats_local
SET data_hora = data_hora || 'Z'
WHERE data_hora NOT LIKE '%+%'
  AND data_hora NOT LIKE '%Z%'
  AND data_hora != '';
