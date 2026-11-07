-- Ampliar senha_hash para TEXT (comportar PBKDF2 Base64 ~44 chars, com margem)
ALTER TABLE usuarios ALTER COLUMN senha_hash TYPE TEXT;

-- Salt por usuário para o PBKDF2 server-side
ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS senha_salt TEXT NOT NULL DEFAULT '';

-- Índice único case-insensitive no email para GetByEmailAsync eficiente
CREATE UNIQUE INDEX IF NOT EXISTS idx_usuarios_email_lower ON usuarios(LOWER(email));
