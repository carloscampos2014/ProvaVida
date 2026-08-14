# ProvaVida — Guia de Deploy

## Visão Geral

O deploy da API é automático via GitHub Actions — qualquer push no `master` que altere arquivos em `src/**` dispara o workflow `deploy-api.yml`.

```
push master → build → testes → publish → rsync → restart → health check → purge Cloudflare
```

---

## 1. Setup Inicial da VM (executar uma única vez)

### 1.1 Acessar a VM

```bash
ssh -i sua-chave.key ubuntu@IP_DA_VM
```

### 1.2 Executar o script de setup

```bash
# Na VM
curl -fsSL https://raw.githubusercontent.com/carloscampos2014/ProvaVida/master/deploy/scripts/setup-vm.sh | sudo bash
```

Ou copiar e executar manualmente:

```bash
sudo bash deploy/scripts/setup-vm.sh
```

### 1.3 Criar banco PostgreSQL

```sql
sudo -u postgres psql
CREATE DATABASE provavida;
CREATE USER provavida_user WITH ENCRYPTED PASSWORD 'SUA_SENHA_SEGURA';
GRANT ALL PRIVILEGES ON DATABASE provavida TO provavida_user;
\q
```

### 1.4 Preencher variáveis de ambiente

```bash
sudo nano /etc/provavida/env
```

Preencher todos os valores marcados como `DEFINIR_`.

### 1.5 Configurar Nginx

O script de setup já cria o server block. Verificar:

```bash
sudo nginx -t
sudo systemctl reload nginx
```

### 1.6 Configurar Cloudflare

No painel Cloudflare:
1. Adicionar registro A: `provida-api` → IP da VM, proxy ativo (laranja)
2. SSL/TLS → modo Full (Strict)
3. O Cloudflare Origin Certificate já deve estar instalado no Nginx do projeto anterior

---

## 2. Configurar GitHub Secrets (executar uma única vez)

No Windows, com GitHub CLI autenticado:

```powershell
.\scripts\setup-github-secrets.ps1
```

Secrets necessários:

| Secret | Descrição |
|---|---|
| `SSH_HOST` | IP da VM OCI |
| `SSH_PORT` | Porta SSH (22 por padrão) |
| `SSH_USER` | `ubuntu` |
| `SSH_KEY` | Conteúdo da chave privada SSH |
| `DB_CONNECTION_STRING` | Connection string PostgreSQL de produção |
| `JWT_SECRET` | Chave JWT (mínimo 32 chars) |
| `EMAIL_HOST` | Host SMTP (ex: smtp.sendgrid.net) |
| `EMAIL_PORT` | Porta SMTP (587) |
| `EMAIL_USUARIO` | Usuário SMTP (ex: apikey) |
| `EMAIL_SENHA` | Senha SMTP ou API Key |
| `EMAIL_REMETENTE` | E-mail remetente |
| `WHATSAPP_TOKEN` | Token WhatsApp Business API |
| `WHATSAPP_PHONE_NUMBER_ID` | Phone Number ID do WhatsApp |
| `CLOUDFLARE_ZONE_ID` | Zone ID do Cloudflare |
| `CLOUDFLARE_API_TOKEN` | Token de API com permissão Cache Purge |

---

## 3. Primeiro Deploy Manual

Após configurar os secrets, basta fazer push no master:

```bash
git push origin master
```

Ou disparar manualmente:

```bash
gh workflow run deploy-api.yml --repo carloscampos2014/ProvaVida
```

---

## 4. APK Release (Android)

### 4.1 Gerar keystore (uma única vez)

```powershell
keytool -genkey -v `
  -keystore provavida.keystore `
  -alias provavida `
  -keyalg RSA -keysize 2048 `
  -validity 10000
```

Guardar o arquivo `.keystore` em local seguro — se perder, não será possível atualizar o app.

### 4.2 Configurar secrets Android

```powershell
.\scripts\setup-github-secrets.ps1 -ApenasAndroid
```

### 4.3 Disparar build do APK

O build dispara automaticamente ao fazer push em `master` com alterações em `mobile/**`, ou manualmente:

```bash
gh workflow run build-apk.yml --repo carloscampos2014/ProvaVida
```

O APK assinado fica disponível como artefato na aba Actions por 30 dias.

---

## 5. Monitoramento

### Status do serviço

```bash
sudo systemctl status provavida-api
```

### Logs em tempo real

```bash
sudo journalctl -u provavida-api -f
```

### Painel Hangfire

Disponível apenas em Development. Em produção, acessar via SSH tunnel:

```bash
ssh -L 9090:localhost:5001 -i sua-chave.key ubuntu@IP_DA_VM
# Então acessar: http://localhost:9090/hangfire
```

### Health check

```bash
curl -H "Host: provida-api.enzojb.com.br" http://localhost/health
```

---

## 6. Rollback

Para reverter para uma versão anterior:

```bash
# Ver últimos commits
git log --oneline -10

# Fazer push de um commit anterior para disparar novo deploy
git revert HEAD
git push origin master
```
