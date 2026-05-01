#!/bin/bash

# Script para deploy em staging do WebApp

set -e

echo "🚀 Iniciando deploy do ProvaVida WebApp em Staging..."

# Build da imagem Docker
echo "📦 Buildando imagem Docker..."
docker build -t provavida-webapp:latest .

# Executar containers
echo "🐳 Iniciando containers..."
docker-compose -f docker-compose.staging.yml up -d

# Aguardar saúde do serviço
echo "⏳ Aguardando serviços ficarem prontos..."
sleep 5

echo "✅ Deploy concluído! WebApp disponível em http://localhost:5173"
echo "📝 Logs: docker-compose -f docker-compose.staging.yml logs -f"
