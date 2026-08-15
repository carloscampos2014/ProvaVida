# Testes de Carga — ProvaVida API

Testes de carga básicos nos endpoints críticos usando [k6](https://k6.io).

## Pré-requisitos

Instalar o k6:

```bash
# Windows (winget)
winget install k6 --source winget

# Windows (choco)
choco install k6

# Linux/macOS
brew install k6
```

## Configuração

Os testes precisam de um usuário real cadastrado na base de produção.
Passe as credenciais via variáveis de ambiente:

```powershell
$env:EMAIL = "seu@email.com"
$env:SENHA = "SuaSenha@123"
```

## Executar

```powershell
# Teste de login
k6 run tests/load/login.js -e EMAIL=$env:EMAIL -e SENHA=$env:SENHA

# Teste de check-in
k6 run tests/load/checkin.js -e EMAIL=$env:EMAIL -e SENHA=$env:SENHA

# Apontar para outro ambiente
k6 run tests/load/checkin.js -e BASE_URL=http://localhost:5000 -e EMAIL=$env:EMAIL -e SENHA=$env:SENHA
```

## Cenários

### login.js

| Estágio | Duração | VUs |
|---------|---------|-----|
| Rampa subida | 30s | 0 → 5 |
| Carga sustentada | 1min | 10 |
| Rampa descida | 20s | 0 |

**Thresholds:**
- `p(95) < 2000ms` — BCrypt é lento por design
- Taxa de sucesso > 95%

### checkin.js

| Estágio | Duração | VUs |
|---------|---------|-----|
| Rampa subida | 30s | 0 → 10 |
| Carga sustentada | 2min | 50 |
| Rampa descida | 30s | 0 |

**Thresholds:**
- `p(95) < 500ms` — RNF definido no projeto
- Taxa de sucesso > 95%

## Interpretar resultados

Ao final de cada execução o k6 exibe um sumário. Os campos importantes:

```
http_req_duration............: avg=120ms  p(90)=180ms  p(95)=210ms  p(99)=350ms
http_req_failed..............: 0.00%
checkin_success_rate.........: 100.00%
```

- **p(95)** — 95% das requisições responderam abaixo desse valor. É o threshold principal.
- **http_req_failed** — deve ser próximo de 0%.
- Se algum threshold falhar, o k6 retorna exit code 1 e exibe `✗` na linha do threshold.

## Documentar resultado

Após rodar, registrar no comentário da issue #33:

```
Ambiente: produção (https://provida-api.enzojb.com.br)
Data: YYYY-MM-DD
VUs máximos: 50 (checkin), 10 (login)

/auth/login
  p50: Xms | p95: Xms | p99: Xms | falhas: X%

/checkin
  p50: Xms | p95: Xms | p99: Xms | falhas: X%

Resultado: ✅ PASSOU / ❌ FALHOU
```
