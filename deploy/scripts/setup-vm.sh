#!/bin/bash
# =============================================================================
# setup-vm.sh — Configura a Oracle Cloud VM para hospedar a ProvaVida API
#
# Uso (na VM):
#   bash setup-vm.sh
#
# O que faz:
#   1. Atualiza o sistema
#   2. Instala .NET 10 runtime
#   3. Instala Nginx (se não estiver instalado)
#   4. Cria estrutura de diretórios
#   5. Cria arquivo de env template (/etc/provavida/env)
#   6. Instala o service systemd provavida-api
#   7. Copia o server block Nginx
#   8. Abre as portas no firewall (ufw)
#
# IMPORTANTE: VM compartilhada — não altera configurações existentes do Nginx
# =============================================================================

set -euo pipefail

GREEN='\033[0;32m'; CYAN='\033[0;36m'; YELLOW='\033[1;33m'; NC='\033[0m'
step() { echo -e "\n${CYAN}[>>] $1${NC}"; }
ok()   { echo -e "${GREEN}[OK] $1${NC}"; }
warn() { echo -e "${YELLOW}[!!] $1${NC}"; }

API_DIR="/opt/provavida/api"
ENV_FILE="/etc/provavida/env"
SERVICE="provavida-api"
PORT=5001

# ── 1. Atualizar sistema ──────────────────────────────────────────────────────
step "Atualizando sistema..."
apt-get update -qq
ok "Sistema atualizado."

# ── 2. Instalar .NET 10 runtime ───────────────────────────────────────────────
step "Verificando .NET 10 runtime..."
if ! dotnet --list-runtimes 2>/dev/null | grep -q "10\."; then
    wget -q https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O /tmp/ms-prod.deb
    dpkg -i /tmp/ms-prod.deb && rm /tmp/ms-prod.deb
    apt-get update -qq
    apt-get install -y aspnetcore-runtime-10.0
    ok ".NET 10 instalado: $(dotnet --version)"
else
    ok ".NET 10 já instalado."
fi

# ── 3. Verificar Nginx ────────────────────────────────────────────────────────
step "Verificando Nginx..."
if ! command -v nginx &>/dev/null; then
    apt-get install -y nginx
    ok "Nginx instalado."
else
    ok "Nginx já instalado."
fi

# ── 4. Criar diretório ────────────────────────────────────────────────────────
step "Criando diretório /opt/provavida/api..."
mkdir -p "$API_DIR"
chown -R www-data:www-data "$API_DIR"
ok "Diretório criado."

# ── 5. Criar arquivo de env template ─────────────────────────────────────────
step "Criando arquivo de env em $ENV_FILE..."
mkdir -p /etc/provavida

if [ ! -f "$ENV_FILE" ]; then
    cat > "$ENV_FILE" << 'ENVEOF'
# ProvaVida API — Variáveis de Ambiente
# ATENÇÃO: Este arquivo contém secrets. Nunca commitar no repositório.
# Editado manualmente ou sobrescrito pelo GitHub Actions a cada deploy.

ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://localhost:5001

ConnectionStrings__Default=Host=localhost;Port=5432;Database=provavida;Username=provavida_user;Password=DEFINIR_SENHA

Jwt__SecretKey=DEFINIR_CHAVE_MINIMO_32_CARACTERES
Jwt__Issuer=ProvaVida
Jwt__Audience=ProvaVida
Jwt__ExpirationHours=24

Email__Host=smtp.sendgrid.net
Email__Port=587
Email__Usuario=apikey
Email__Senha=DEFINIR_SENDGRID_KEY
Email__NomeRemetente=ProvaVida
Email__EmailRemetente=noreply@enzojb.com.br

WhatsApp__Token=DEFINIR_TOKEN
WhatsApp__PhoneNumberId=DEFINIR_PHONE_NUMBER_ID
ENVEOF
    chmod 600 "$ENV_FILE"
    chown root:root "$ENV_FILE"
    warn "IMPORTANTE: edite $ENV_FILE com os valores reais antes de iniciar a API!"
    warn "  sudo nano $ENV_FILE"
else
    ok "$ENV_FILE já existe — não sobrescrito."
fi

# ── 6. Instalar service systemd ───────────────────────────────────────────────
step "Instalando service systemd ${SERVICE}..."
cat > "/etc/systemd/system/${SERVICE}.service" << SERVICEEOF
[Unit]
Description=ProvaVida API
After=network.target
Wants=network-online.target

[Service]
Type=notify
User=www-data
WorkingDirectory=${API_DIR}
ExecStart=/usr/bin/dotnet ${API_DIR}/ProvaVida.Api.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=${SERVICE}
EnvironmentFile=${ENV_FILE}

NoNewPrivileges=true
PrivateTmp=true
ProtectSystem=full

[Install]
WantedBy=multi-user.target
SERVICEEOF

systemctl daemon-reload
ok "Service ${SERVICE}.service instalado."

# ── 7. Configurar Nginx (server block adicional — não altera configs existentes)
step "Configurando Nginx server block..."
NGINX_CONF="/etc/nginx/sites-available/provida-api"

if [ ! -f "$NGINX_CONF" ]; then
    cat > "$NGINX_CONF" << NGINXEOF
server {
    listen 80;
    server_name provida-api.enzojb.com.br;

    client_max_body_size 10M;

    add_header X-Frame-Options      "DENY"    always;
    add_header X-Content-Type-Options "nosniff" always;

    location / {
        proxy_pass         http://localhost:${PORT};
        proxy_http_version 1.1;
        proxy_set_header   Host             \$host;
        proxy_set_header   X-Real-IP        \$remote_addr;
        proxy_set_header   X-Forwarded-For  \$proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto \$scheme;
        proxy_set_header   CF-Connecting-IP \$http_cf_connecting_ip;
        proxy_read_timeout 90s;
    }
}
NGINXEOF
    ln -sf "$NGINX_CONF" /etc/nginx/sites-enabled/provida-api
    nginx -t && systemctl reload nginx
    ok "Nginx configurado para provida-api.enzojb.com.br → localhost:${PORT}."
else
    ok "Nginx server block já existe — não sobrescrito."
fi

# ── 8. Banco de dados ─────────────────────────────────────────────────────────
step "Verificando banco de dados..."
warn "Certifique-se de criar o banco e o usuário PostgreSQL manualmente:"
echo "  sudo -u postgres psql"
echo "  CREATE DATABASE provavida;"
echo "  CREATE USER provavida_user WITH ENCRYPTED PASSWORD 'SUA_SENHA';"
echo "  GRANT ALL PRIVILEGES ON DATABASE provavida TO provavida_user;"

echo ""
ok "Setup da VM concluído!"
echo ""
echo "Próximos passos:"
echo "  1. Editar /etc/provavida/env com os valores reais"
echo "  2. Criar banco PostgreSQL (ver acima)"
echo "  3. Adicionar registro A 'provida-api' no Cloudflare → IP desta VM"
echo "  4. Configurar GitHub Secrets (ver scripts/setup-github-secrets.ps1)"
echo "  5. Fazer push no master para disparar o deploy automático"
echo ""
echo "Status do serviço após o primeiro deploy:"
echo "  sudo systemctl status ${SERVICE}"
echo "  sudo journalctl -u ${SERVICE} -f"
