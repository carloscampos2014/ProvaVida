# 🧪 Guia de Testes da API ProvaVida

## 🚀 Como Executar a API

### 1. Iniciar o servidor
```bash
# Na raiz do projeto
dotnet run --project src/ProvaVida.API/

# Ou com watch mode (reinicia ao salvar)
dotnet watch --project src/ProvaVida.API/
```

**Saída esperada:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5176
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to stop.
```

### 2. Acessar Swagger/OpenAPI
Abra no navegador:
```
http://localhost:5176/swagger
```

## 🧪 Ferramentas para Testar

### Opção 1: VS Code + REST Client Extension
1. Instale a extensão "REST Client" (Huachao Mao)
2. Abra o arquivo `ProvaVida.API.http`
3. Clique em "Send Request" acima de cada requisição
4. Veja a resposta no painel lado direito

### Opção 2: Postman
1. Abra Postman
2. Importe o Swagger: `http://localhost:5176/swagger/v1/swagger.json`
3. Teste os endpoints

### Opção 3: cURL (Terminal)
```bash
# Registrar usuário
curl -X POST http://localhost:5176/auth/registrar \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "João Silva",
    "email": "joao@exemplo.com",
    "telefone": "11987654321",
    "senha": "SenhaForte@123",
    "contatoEmergencia": {
      "nome": "Maria Silva",
      "email": "maria@exemplo.com",
      "whatsApp": "11987654322",
      "prioridade": 1
    }
  }'
```

### Opção 4: Thunder Client (VS Code)
1. Instale a extensão "Thunder Client"
2. Importe `ProvaVida.API.http`
3. Execute os testes

## 📝 Fluxo Recomendado de Testes

### ✅ Teste 1: Registrar Novo Usuário (Sucesso)
```http
POST http://localhost:5176/auth/registrar
Content-Type: application/json

{
  "nome": "João Silva",
  "email": "joao@exemplo.com",
  "telefone": "11987654321",
  "senha": "SenhaForte@123",
  "contatoEmergencia": {
    "nome": "Maria Silva",
    "email": "maria@exemplo.com",
    "whatsApp": "11987654322",
    "prioridade": 1
  }
}
```

**Resposta esperada (200 OK):**
```json
{
  "sucesso": true,
  "dados": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "nome": "João Silva",
    "email": "joao@exemplo.com",
    "telefone": "11987654321"
  },
  "mensagem": "Usuário registrado com sucesso"
}
```

**⚠️ Salve o `id` para os próximos testes!**

---

### ❌ Teste 2: Registrar com Senha Fraca (Erro de Validação)
```http
POST http://localhost:5176/auth/registrar
Content-Type: application/json

{
  "nome": "Carlos Santos",
  "email": "carlos@exemplo.com",
  "telefone": "11987654323",
  "senha": "123456",
  "contatoEmergencia": {
    "nome": "Ana Santos",
    "email": "ana@exemplo.com",
    "whatsApp": "11987654324",
    "prioridade": 1
  }
}
```

**Resposta esperada (400 Bad Request):**
```json
{
  "sucesso": false,
  "erros": [
    "Senha deve ter no mínimo 8 caracteres",
    "Senha deve conter pelo menos uma letra maiúscula",
    "Senha deve conter pelo menos um dígito",
    "Senha deve conter pelo menos um caractere especial"
  ],
  "statusCode": 400
}
```

---

### ❌ Teste 3: Registrar sem Contato de Emergência (Erro de Validação)
```http
POST http://localhost:5176/auth/registrar
Content-Type: application/json

{
  "nome": "Pedro Oliveira",
  "email": "pedro@exemplo.com",
  "telefone": "11987654325",
  "senha": "SenhaForte@123"
}
```

**Resposta esperada (400 Bad Request):**
```json
{
  "sucesso": false,
  "erros": [
    "Contato de emergência é obrigatório"
  ],
  "statusCode": 400
}
```

---

### ❌ Teste 4: Registrar com Telefone Inválido (Erro de Validação)
```http
POST http://localhost:5176/auth/registrar
Content-Type: application/json

{
  "nome": "Lucas Pereira",
  "email": "lucas@exemplo.com",
  "telefone": "1234567890",
  "senha": "SenhaForte@123",
  "contatoEmergencia": {
    "nome": "Fabio Pereira",
    "email": "fabio@exemplo.com",
    "whatsApp": "11987654326",
    "prioridade": 1
  }
}
```

**Resposta esperada (400 Bad Request):**
```json
{
  "sucesso": false,
  "erros": [
    "Telefone deve estar no formato de celular brasileiro: 11 9XXXXXXXX"
  ],
  "statusCode": 400
}
```

**Formatos aceitos:**
- `11987654321` (sem formatação)
- `(11) 98765-4321` (com parêntese e hífen)
- `11 98765-4321` (com espaço e hífen)

---

### ✅ Teste 5: Fazer Login (Sucesso)
```http
POST http://localhost:5176/auth/login
Content-Type: application/json

{
  "email": "joao@exemplo.com",
  "senha": "SenhaForte@123"
}
```

**Resposta esperada (200 OK):**
```json
{
  "sucesso": true,
  "dados": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "nome": "João Silva",
    "email": "joao@exemplo.com",
    "telefone": "11987654321"
  },
  "mensagem": "Login realizado com sucesso"
}
```

---

### ❌ Teste 6: Login com Senha Incorreta (Erro)
```http
POST http://localhost:5176/auth/login
Content-Type: application/json

{
  "email": "joao@exemplo.com",
  "senha": "SenhaErrada@123"
}
```

**Resposta esperada (401 Unauthorized):**
```json
{
  "sucesso": false,
  "erro": "Senha incorreta.",
  "statusCode": 401
}
```

---

### ✅ Teste 7: Registrar Check-in
```http
POST http://localhost:5176/check-ins/registrar
Content-Type: application/json

{
  "usuarioId": "550e8400-e29b-41d4-a716-446655440000",
  "latitude": -23.5505,
  "longitude": -46.6333,
  "descricao": "Check-in na Avenida Paulista"
}
```

**Resposta esperada (201 Created):**
```json
{
  "sucesso": true,
  "dados": {
    "id": "660e8400-e29b-41d4-a716-446655440111",
    "usuarioId": "550e8400-e29b-41d4-a716-446655440000",
    "latitude": -23.5505,
    "longitude": -46.6333,
    "descricao": "Check-in na Avenida Paulista",
    "dataCriacao": "2026-02-01T10:30:00Z",
    "status": 0
  },
  "mensagem": "Check-in registrado com sucesso"
}
```

---

### ✅ Teste 8: Obter Histórico de Check-ins
```http
GET http://localhost:5176/check-ins/historico/550e8400-e29b-41d4-a716-446655440000
Accept: application/json
```

**Resposta esperada (200 OK):**
```json
{
  "sucesso": true,
  "dados": [
    {
      "id": "660e8400-e29b-41d4-a716-446655440111",
      "usuarioId": "550e8400-e29b-41d4-a716-446655440000",
      "latitude": -23.5505,
      "longitude": -46.6333,
      "descricao": "Check-in na Avenida Paulista",
      "dataCriacao": "2026-02-01T10:30:00Z",
      "status": 0
    }
  ],
  "mensagem": "Histórico recuperado"
}
```

---

### ✅ Teste 9: Criar Contato de Emergência
```http
POST http://localhost:5176/contatos
Content-Type: application/json

{
  "usuarioId": "550e8400-e29b-41d4-a716-446655440000",
  "nome": "Contato Adicional",
  "email": "contato.extra@exemplo.com",
  "whatsApp": "11987654329",
  "prioridade": 2
}
```

**Resposta esperada (201 Created):**
```json
{
  "sucesso": true,
  "dados": {
    "id": "770e8400-e29b-41d4-a716-446655440222",
    "usuarioId": "550e8400-e29b-41d4-a716-446655440000",
    "nome": "Contato Adicional",
    "email": "contato.extra@exemplo.com",
    "whatsApp": "11987654329",
    "prioridade": 2,
    "ativo": true,
    "dataCriacao": "2026-02-01T10:35:00Z"
  },
  "mensagem": "Contato criado com sucesso"
}
```

---

### ✅ Teste 10: Listar Contatos de Usuário
```http
GET http://localhost:5176/contatos/550e8400-e29b-41d4-a716-446655440000
Accept: application/json
```

**Resposta esperada (200 OK):**
```json
{
  "sucesso": true,
  "dados": [
    {
      "id": "660e8400-e29b-41d4-a716-446655440111",
      "usuarioId": "550e8400-e29b-41d4-a716-446655440000",
      "nome": "Maria Silva",
      "email": "maria@exemplo.com",
      "whatsApp": "11987654322",
      "prioridade": 1,
      "ativo": true,
      "dataCriacao": "2026-02-01T10:25:00Z"
    },
    {
      "id": "770e8400-e29b-41d4-a716-446655440222",
      "usuarioId": "550e8400-e29b-41d4-a716-446655440000",
      "nome": "Contato Adicional",
      "email": "contato.extra@exemplo.com",
      "whatsApp": "11987654329",
      "prioridade": 2,
      "ativo": true,
      "dataCriacao": "2026-02-01T10:35:00Z"
    }
  ],
  "mensagem": "Contatos recuperados"
}
```

---

### ✅ Teste 11: Atualizar Contato
```http
PUT http://localhost:5176/contatos/660e8400-e29b-41d4-a716-446655440111
Content-Type: application/json

{
  "nome": "Maria Silva Atualizado",
  "email": "maria.novo@exemplo.com",
  "whatsApp": "11987654332",
  "prioridade": 1
}
```

**Resposta esperada (200 OK):**
```json
{
  "sucesso": true,
  "dados": {
    "id": "660e8400-e29b-41d4-a716-446655440111",
    "usuarioId": "550e8400-e29b-41d4-a716-446655440000",
    "nome": "Maria Silva Atualizado",
    "email": "maria.novo@exemplo.com",
    "whatsApp": "11987654332",
    "prioridade": 1,
    "ativo": true,
    "dataCriacao": "2026-02-01T10:25:00Z"
  },
  "mensagem": "Contato atualizado com sucesso"
}
```

---

### ✅ Teste 12: Deletar Contato
```http
DELETE http://localhost:5176/contatos/660e8400-e29b-41d4-a716-446655440111
```

**Resposta esperada (200 OK ou 204 No Content):**
```json
{
  "sucesso": true,
  "mensagem": "Contato deletado com sucesso"
}
```

---

## 🔑 Requisitos de Validação

### Senha Forte
- Mínimo **8 caracteres**
- Pelo menos **1 letra maiúscula** (A-Z)
- Pelo menos **1 letra minúscula** (a-z)
- Pelo menos **1 dígito** (0-9)
- Pelo menos **1 caractere especial** (!@#$%^&*()...)

**✅ Exemplo válido:** `SenhaForte@123`
**❌ Exemplos inválidos:** `123456`, `senha123`, `SENHA123!`

### Telefone Brasileiro
- Formato: `11` (DDD) + `9` (celular) + `8 dígitos`
- Aceita formatos:
  - `11987654321` (sem formatação)
  - `(11) 98765-4321` (com parêntese e hífen)
  - `11 98765-4321` (com espaço e hífen)

### Email
- Deve ser válido (RFC 5322 simplified)
- Deve ser único no sistema

### Contato de Emergência
- **Obrigatório** no registro
- Telefone WhatsApp também em formato brasileiro
- Prioridade: 1-10

## 📊 Status Codes Esperados

| Código | Significado | Exemplo |
|--------|------------|---------|
| 200 | OK - Requisição bem-sucedida | Login, listar, atualizar |
| 201 | Created - Recurso criado | Registro, criar contato |
| 204 | No Content - Sucesso sem corpo | Deletar |
| 400 | Bad Request - Validação falhou | Senha fraca, email inválido |
| 401 | Unauthorized - Senha incorreta | Login com senha errada |
| 403 | Forbidden - Usuário inativo | Verificar status do usuário |
| 404 | Not Found - Recurso não existe | ID inválido |
| 409 | Conflict - Email duplicado | Registrar com email existente |
| 422 | Unprocessable Entity - Erro de domínio | Dados inconsistentes |
| 500 | Internal Server Error | Erro inesperado |

## 🐛 Troubleshooting

### Erro: "Connection refused"
- Verifique se a API está rodando: `dotnet run --project src/ProvaVida.API/`
- Verifique se a porta 5176 está em uso

### Erro: "Invalid JSON"
- Certifique-se que o JSON está válido (use um validador JSON online)
- Verifique aspas e pontos-e-vírgula

### Erro: "Usuário já existe"
- O email já foi registrado em outro teste
- Use um novo email ou delete o banco e tente novamente

### Erro: "Contato não encontrado"
- O ID do contato pode estar inválido
- Verifique se foi criado antes

## 📦 Resetar Banco de Dados

### Deletar banco SQLite
```bash
# No diretório raiz do projeto
rm -f ProvaVida.db
rm -f ProvaVida.db-shm
rm -f ProvaVida.db-wal
```

Depois execute a API novamente - o banco será recriado automaticamente.

## 📚 Referências

- [Arquivo ProvaVida.API.http](src/ProvaVida.API/ProvaVida.API.http) - Suite de testes REST
- [Swagger Local](http://localhost:5176/swagger) - Documentação interativa
- [Documentação Clean Architecture](docs/ARQUITETURA.md)
- [Guia de Testes](TESTES_API.md) - Este arquivo

---

**Última atualização:** 1 de fevereiro de 2026  
**Versão:** 1.1-Sprint4
