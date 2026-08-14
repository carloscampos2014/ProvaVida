<#
.SYNOPSIS
    Configura os GitHub Secrets necessários para CI/CD do ProvaVida.

.DESCRIPTION
    Cria todos os secrets no repositório GitHub via GitHub CLI.
    Execute este script uma única vez antes do primeiro deploy.

    Secrets para Deploy da API:
      SSH_HOST, SSH_PORT, SSH_USER, SSH_KEY
      DB_CONNECTION_STRING, JWT_SECRET
      EMAIL_HOST, EMAIL_PORT, EMAIL_USUARIO, EMAIL_SENHA, EMAIL_REMETENTE
      WHATSAPP_TOKEN, WHATSAPP_PHONE_NUMBER_ID
      CLOUDFLARE_ZONE_ID, CLOUDFLARE_API_TOKEN

    Secrets para APK Release:
      ANDROID_KEYSTORE_BASE64, ANDROID_KEY_ALIAS,
      ANDROID_KEY_PASSWORD, ANDROID_STORE_PASSWORD

.NOTES
    Pré-requisitos:
    - GitHub CLI (gh) autenticado: gh auth login
    - Chave SSH da VM Oracle Cloud disponível localmente

.EXAMPLE
    # Configurar secrets da API:
    .\scripts\setup-github-secrets.ps1

    # Configurar apenas secrets Android (após gerar a keystore):
    .\scripts\setup-github-secrets.ps1 -ApenasAndroid
#>
param(
    [switch]$ApenasAndroid
)

$repo = "carloscampos2014/ProvaVida"

Write-Host "`n[>>] Configurando GitHub Secrets para $repo`n" -ForegroundColor Cyan

if ($ApenasAndroid) {
    Write-Host "[>>] Modo: apenas secrets Android`n" -ForegroundColor Yellow
}

# ── SSH ───────────────────────────────────────────────────────────────────────
if (-not $ApenasAndroid) {
    Write-Host "[1/11] SSH_HOST..." -ForegroundColor Yellow
    $sshHost = Read-Host "IP da VM Oracle Cloud"
    gh secret set SSH_HOST --body $sshHost --repo $repo

    Write-Host "[2/11] SSH_PORT..." -ForegroundColor Yellow
    $sshPort = Read-Host "Porta SSH (padrão 22)"
    if ([string]::IsNullOrWhiteSpace($sshPort)) { $sshPort = "22" }
    gh secret set SSH_PORT --body $sshPort --repo $repo

    Write-Host "[3/11] SSH_USER..." -ForegroundColor Yellow
    $sshUser = Read-Host "Usuário SSH (padrão ubuntu)"
    if ([string]::IsNullOrWhiteSpace($sshUser)) { $sshUser = "ubuntu" }
    gh secret set SSH_USER --body $sshUser --repo $repo

    Write-Host "[4/11] SSH_KEY..." -ForegroundColor Yellow
    $sshKeyPath = Read-Host "Caminho da chave SSH privada (.key ou .pem)"
    if (Test-Path $sshKeyPath) {
        $sshKeyContent = Get-Content $sshKeyPath -Raw
        gh secret set SSH_KEY --body $sshKeyContent --repo $repo
        Write-Host "     Chave SSH carregada." -ForegroundColor Gray
    } else {
        Write-Host "[!!] Chave SSH não encontrada em $sshKeyPath" -ForegroundColor Red
    }

    Write-Host "[4b] SSH_KNOWN_HOST (fingerprint do host — evita MITM no CI/CD)..." -ForegroundColor Yellow
    Write-Host "     Execute na sua maquina local (que ja confia na VM):" -ForegroundColor Gray
    Write-Host "       ssh-keyscan -p $sshPort -H $sshHost" -ForegroundColor Gray
    Write-Host "     Cole o resultado abaixo (linha completa do known_hosts):" -ForegroundColor Gray
    $sshKnownHost = Read-Host "known_host entry"
    if (-not [string]::IsNullOrWhiteSpace($sshKnownHost)) {
        gh secret set SSH_KNOWN_HOST --body $sshKnownHost --repo $repo
        Write-Host "     SSH_KNOWN_HOST configurado." -ForegroundColor Gray
    } else {
        Write-Host "[!!] SSH_KNOWN_HOST nao configurado. O deploy vai falhar por seguranca." -ForegroundColor Red
    }

    # ── Banco de dados ────────────────────────────────────────────────────────
    Write-Host "[5/11] DB_CONNECTION_STRING..." -ForegroundColor Yellow
    $dbHost   = Read-Host "Host PostgreSQL (padrão localhost)"
    if ([string]::IsNullOrWhiteSpace($dbHost)) { $dbHost = "localhost" }
    $dbName   = Read-Host "Database (padrão provavida)"
    if ([string]::IsNullOrWhiteSpace($dbName)) { $dbName = "provavida" }
    $dbUser   = Read-Host "Usuário PostgreSQL (padrão provavida_user)"
    if ([string]::IsNullOrWhiteSpace($dbUser)) { $dbUser = "provavida_user" }
    $dbPass   = Read-Host "Senha PostgreSQL" -AsSecureString
    $dbPassPl = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
        [Runtime.InteropServices.Marshal]::SecureStringToBSTR($dbPass))
    $connStr  = "Host=${dbHost};Port=5432;Database=${dbName};Username=${dbUser};Password=${dbPassPl}"
    gh secret set DB_CONNECTION_STRING --body $connStr --repo $repo

    # ── JWT ───────────────────────────────────────────────────────────────────
    Write-Host "[6/11] JWT_SECRET..." -ForegroundColor Yellow
    $jwtSecret = -join ((65..90) + (97..122) + (48..57) |
        Get-Random -Count 64 | ForEach-Object { [char]$_ })
    # Não exibir o secret no console — copie-o do painel do GitHub após configurar
    gh secret set JWT_SECRET --body $jwtSecret --repo $repo
    Write-Host "     JWT_SECRET configurado (64 chars). Copie-o em: https://github.com/$repo/settings/secrets/actions" -ForegroundColor Gray
    Write-Host "     AVISO: O valor nao sera exibido novamente." -ForegroundColor Yellow

    # ── E-mail SMTP ───────────────────────────────────────────────────────────
    Write-Host "[7/11] Email (SMTP)..." -ForegroundColor Yellow
    $emailHost  = Read-Host "SMTP Host (ex: smtp.sendgrid.net)"
    gh secret set EMAIL_HOST --body $emailHost --repo $repo
    $emailPort  = Read-Host "SMTP Port (padrão 587)"
    if ([string]::IsNullOrWhiteSpace($emailPort)) { $emailPort = "587" }
    gh secret set EMAIL_PORT --body $emailPort --repo $repo
    $emailUser  = Read-Host "SMTP Usuario (ex: apikey para SendGrid)"
    gh secret set EMAIL_USUARIO --body $emailUser --repo $repo
    $emailPass  = Read-Host "SMTP Senha/API Key" -AsSecureString
    $emailPassPl = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
        [Runtime.InteropServices.Marshal]::SecureStringToBSTR($emailPass))
    gh secret set EMAIL_SENHA --body $emailPassPl --repo $repo
    $emailFrom  = Read-Host "E-mail remetente (ex: noreply@enzojb.com.br)"
    gh secret set EMAIL_REMETENTE --body $emailFrom --repo $repo

    # ── WhatsApp Business API ─────────────────────────────────────────────────
    Write-Host "[8/11] WhatsApp Business API..." -ForegroundColor Yellow
    $waToken  = Read-Host "WhatsApp Token (Bearer)"
    gh secret set WHATSAPP_TOKEN --body $waToken --repo $repo
    $waPhone  = Read-Host "WhatsApp Phone Number ID"
    gh secret set WHATSAPP_PHONE_NUMBER_ID --body $waPhone --repo $repo

    # ── Cloudflare ────────────────────────────────────────────────────────────
    Write-Host "[9/11] Cloudflare Zone ID..." -ForegroundColor Yellow
    $cfZone  = Read-Host "Cloudflare Zone ID (painel Cloudflare → Overview → API)"
    gh secret set CLOUDFLARE_ZONE_ID --body $cfZone --repo $repo

    Write-Host "[10/11] Cloudflare API Token..." -ForegroundColor Yellow
    $cfToken = Read-Host "Cloudflare API Token (Cache Purge permission)"
    gh secret set CLOUDFLARE_API_TOKEN --body $cfToken --repo $repo

    Write-Host "[11/11] Secrets da API configurados!" -ForegroundColor Green
}

# ── Android ───────────────────────────────────────────────────────────────────
Write-Host "`n[>>] Secrets Android (Build APK Release)" -ForegroundColor Cyan
Write-Host ""
Write-Host "Para gerar uma keystore, execute:" -ForegroundColor Gray
Write-Host "  keytool -genkey -v -keystore provavida.keystore -alias provavida -keyalg RSA -keysize 2048 -validity 10000"
Write-Host ""

$keystorePath = Read-Host "Caminho para o arquivo .keystore (Enter para pular)"

if ($keystorePath -and (Test-Path $keystorePath)) {
    Write-Host "[A1/4] ANDROID_KEYSTORE_BASE64..." -ForegroundColor Yellow
    $keystoreBase64 = [Convert]::ToBase64String([IO.File]::ReadAllBytes($keystorePath))
    gh secret set ANDROID_KEYSTORE_BASE64 --body $keystoreBase64 --repo $repo

    Write-Host "[A2/4] ANDROID_KEY_ALIAS..." -ForegroundColor Yellow
    $keyAlias = Read-Host "Alias da chave (ex: provavida)"
    gh secret set ANDROID_KEY_ALIAS --body $keyAlias --repo $repo

    Write-Host "[A3/4] ANDROID_KEY_PASSWORD..." -ForegroundColor Yellow
    $keyPass = Read-Host "Senha da chave" -AsSecureString
    $keyPassPl = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
        [Runtime.InteropServices.Marshal]::SecureStringToBSTR($keyPass))
    gh secret set ANDROID_KEY_PASSWORD --body $keyPassPl --repo $repo

    Write-Host "[A4/4] ANDROID_STORE_PASSWORD..." -ForegroundColor Yellow
    $storePass = Read-Host "Senha da keystore" -AsSecureString
    $storePassPl = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
        [Runtime.InteropServices.Marshal]::SecureStringToBSTR($storePass))
    gh secret set ANDROID_STORE_PASSWORD --body $storePassPl --repo $repo

    Write-Host "`n[OK] Secrets Android configurados!" -ForegroundColor Green
} else {
    Write-Host "[!!] Keystore não fornecida. Secrets Android não configurados." -ForegroundColor Red
}

Write-Host "`n[OK] Configuracao concluida!" -ForegroundColor Green
Write-Host "     Verifique os secrets em: https://github.com/$repo/settings/secrets/actions"
