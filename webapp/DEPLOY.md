# Deploy em Staging

## Pré-requisitos
- Docker e Docker Compose instalados
- Backend (.NET 9) rodando ou disponível via container

## Deploy Manual

1. Clone o repositório e acesse a pasta `webapp`:
```bash
cd webapp
```

2. Crie o arquivo `.env` baseado em `.env.example`:
```bash
cp .env.example .env
# Edite .env conforme necessário
```

3. Execute o script de deploy:
```bash
bash scripts/deploy-staging.sh
```

4. Acesse o WebApp em `http://localhost:5173`

## Deploy via Docker Compose (sem script)

```bash
docker-compose -f docker-compose.staging.yml up -d
```

## Logs e Monitoramento

Ver logs:
```bash
docker-compose -f docker-compose.staging.yml logs -f webapp
```

Parar containers:
```bash
docker-compose -f docker-compose.staging.yml down
```

## Variáveis de Ambiente

Configure conforme ambiente (dev/staging/prod):
- `VITE_API_URL` — URL da API backend
- `VITE_ENV` — Ambiente (development, staging, production)

---

Para mais informações, consulte o README.md da raiz do projeto.
