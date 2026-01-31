# User Stories - ProvaVida

**Última Atualização:** 31 de janeiro de 2026  
**Versão:** 1.0  
**Foco:** Check-in e Alertas de Emergência

---

## 📖 Padrão de User Stories

```
Como [ATOR]
Eu quero [AÇÃO]
Para que [BENEFÍCIO]

Critérios de Aceitação:
- [ ] Critério 1
- [ ] Critério 2
- [ ] Critério 3
```

---

## 🎯 Épico E01 - Fundação do Domínio

### **US-01: Criar Entidade Usuario**

**Como** Desenvolvedor  
**Eu quero** criar a entidade `Usuario` no domínio  
**Para que** o sistema tenha uma representação digital de um usuário

#### Critérios de Aceitação:
- [ ] Entidade `Usuario` possui propriedades: `Id`, `Nome`, `Email`, `Telefone`, `DataCriacao`, `Ativo`
- [ ] Propriedade `DataProximoCheckIn` armazena o próximo prazo de 48h
- [ ] Construtores validam dados obrigatórios
- [ ] Entidade é imutável após criação (usar propriedades privadas com getters)

#### Tarefas Técnicas:
- Criar arquivo `src/ProvaVida.Dominio/Entidades/Usuario.cs`
- Implementar validações de negócio
- Criar testes unitários em `tests/ProvaVida.Dominio.Tests/UsuarioTests.cs`

#### Estimativa: 2h

---

### **US-02: Criar Entidade ContatoEmergencia**

**Como** Desenvolvedor  
**Eu quero** criar a entidade `ContatoEmergencia` no domínio  
**Para que** o sistema possa registrar contatos que recebam alertas

#### Critérios de Aceitação:
- [ ] Entidade `ContatoEmergencia` possui: `Id`, `UsuarioId`, `Nome`, `Email`, `WhatsApp`, `Ativo`, `DataCadastro`
- [ ] Valida email e telefone com formato correto
- [ ] Não permite criar contato sem usuário associado
- [ ] Suporta múltiplos contatos por usuário

#### Tarefas Técnicas:
- Criar arquivo `src/ProvaVida.Dominio/Entidades/ContatoEmergencia.cs`
- Validar formatos de email (regex)
- Testes unitários

#### Estimativa: 2h

---

### **US-03: Criar Entidade CheckIn**

**Como** Desenvolvedor  
**Eu quero** criar a entidade `CheckIn` para registrar provas de vida  
**Para que** o sistema rastreie quando o usuário se manifestou

#### Critérios de Aceitação:
- [ ] Entidade `CheckIn` possui: `Id`, `UsuarioId`, `DataCheckIn`, `DataProximoVencimento`
- [ ] `DataProximoVencimento` é calculada como `DataCheckIn + 48 horas`
- [ ] Método `EstaVencido()` retorna true se agora > `DataProximoVencimento`
- [ ] Imutável após criação

#### Tarefas Técnicas:
- Criar `src/ProvaVida.Dominio/Entidades/CheckIn.cs`
- Implementar cálculo de 48h com `DateTime`
- Testes para validar cálculo correto

#### Estimativa: 2h

---

### **US-04: Criar Entidade Notificacao**

**Como** Desenvolvedor  
**Eu quero** criar a entidade `Notificacao` para registrar alertas  
**Para que** o sistema mantenha histórico de lembretes e emergências

#### Critérios de Aceitação:
- [ ] Entidade `Notificacao` possui: `Id`, `ContatoEmergenciaId`, `TipoNotificacao`, `MeioNotificacao`, `DataEnvio`, `Status`
- [ ] `TipoNotificacao`: LEMBRETE_6H, LEMBRETE_2H, EMERGENCIA
- [ ] `MeioNotificacao`: EMAIL, WHATSAPP
- [ ] `Status`: PENDENTE, ENVIADA, ERRO

#### Tarefas Técnicas:
- Criar `src/ProvaVida.Dominio/Entidades/Notificacao.cs`
- Criar Enums (`TipoNotificacao`, `MeioNotificacao`, `StatusNotificacao`)
- Testes

#### Estimativa: 2h

---

## 🎯 Épico E02 - Gestão de Usuários

### **US-05: Cadastrar Usuário com Contato de Emergência (Obrigatório)**

**Como** Usuário  
**Eu quero** me registrar no sistema com meu contato de emergência  
**Para que** o monitoramento seja ativado e eu receba alertas

#### Critérios de Aceitação:
- [ ] Não é possível criar usuário sem pelo menos 1 contato de emergência
- [ ] Contato deve ter Nome, Email e WhatsApp válidos
- [ ] Sistema valida email (RFC 5322) e telefone (formato brasileiro)
- [ ] Após cadastro, `DataProximoCheckIn` é definida como agora + 48h
- [ ] Resposta da API retorna dados cadastrados com ID gerado

#### Casos de Teste:
1. **Sucesso:** Criar usuário com 1 contato válido → Status 201, ID retornado
2. **Erro:** Criar usuário sem contatos → Status 400, mensagem "Contato de emergência obrigatório"
3. **Erro:** Email inválido → Status 422, mensagem "Email inválido"

#### Tarefas Técnicas:
- `ServicoUsuario.CadastrarAsync(dtoUsuario)`
- `ValidadorEmail.ValidarAsync(email)`
- `ValidadorTelefone.ValidarAsync(whatsapp)`
- `ControladorUsuario.Post([FromBody] CadastroUsuarioDTO)`
- Testes de integração com banco

#### Estimativa: 5h

---

## 🎯 Épico E03 - Check-in

### **US-06: Realizar Check-in e Resetar Prazo de 48h**

**Como** Usuário  
**Eu quero** fazer um check-in para confirmar que estou bem  
**Para que** o prazo de vencimento seja estendido por mais 48 horas

#### Fluxo Principal:
1. Usuário faz requisição POST `/check-ins`
2. Sistema valida se usuário existe e está ativo
3. Cria novo registro de `CheckIn` com `DataProximoVencimento = Agora + 48h`
4. Limpa alertas pendentes para este usuário
5. Retorna confirmação com novo prazo

#### Critérios de Aceitação:
- [ ] Check-in criado com timestamp UTC
- [ ] `DataProximoVencimento` é exatamente 48 horas no futuro
- [ ] Notificações pendentes (LEMBRETE_6H, LEMBRETE_2H) são canceladas
- [ ] Histórico de check-ins é limitado a 5 registros (FIFO)
- [ ] Resposta inclui: `CheckInId`, `DataCheckIn`, `DataProximoVencimento`, `ProximoAlerte`

#### Casos de Teste:
1. **Sucesso:** Check-in válido → Status 201, prazo estendido
2. **Erro:** Usuário não existe → Status 404
3. **Erro:** Usuário inativo → Status 403
4. **Sucesso:** Histórico limitado a 5 → 6º check-in remove o 1º

#### Tarefas Técnicas:
- `ServicoCheckIn.RegistrarCheckInAsync(usuarioId)`
- `RepositorioCheckIn.LimparHistoricoExcedente(usuarioId, limite=5)`
- `ControladorCheckIn.Post([FromQuery] string usuarioId)`
- Testes com mock de DateTime

#### Estimativa: 6h

---

### **US-07: Listar Histórico de Check-ins (Últimos 5)**

**Como** Usuário  
**Eu quero** visualizar meus últimos check-ins  
**Para que** eu saiba quando foi meu último contato com o sistema

#### Critérios de Aceitação:
- [ ] Retorna no máximo 5 registros de check-in mais recentes
- [ ] Ordena por `DataCheckIn` DESC
- [ ] Inclui `DataProximoVencimento` em cada registro
- [ ] Calcula dias/horas até vencimento para cada check-in
- [ ] Usuário não autenticado recebe erro 401

#### Casos de Teste:
1. **Sucesso:** 3 check-ins cadastrados → Retorna 3, ordenados DESC
2. **Sucesso:** 8 check-ins cadastrados → Retorna apenas 5 mais recentes
3. **Vazio:** Nenhum check-in → Status 200, array vazio

#### Tarefas Técnicas:
- `ServicoCheckIn.ListarHistoricoAsync(usuarioId, limite=5)`
- `ControladorCheckIn.Get([FromQuery] string usuarioId)`
- DTO com campos: `CheckInId`, `DataCheckIn`, `DataProximoVencimento`, `DiasAteVencimento`

#### Estimativa: 3h

---

## 🎯 Épico E04 - Alertas e Notificações

### **US-08: Gerar Alerta de Lembrete (-6h antes do vencimento)**

**Como** Sistema  
**Eu quero** calcular quando faltam 6 horas para vencimento  
**Para que** eu dispare um lembrete ao usuário

#### Regra de Negócio:
- Lembrete deve ser disparado quando: `Agora >= (DataProximoVencimento - 6 horas)`
- Apenas 1 lembrete de -6h por ciclo de 48h
- Se usuário fizer check-in antes do lembrete, este é cancelado

#### Critérios de Aceitação:
- [ ] Job/Scheduler verifica a cada 10 minutos
- [ ] Cria registro `Notificacao` com `TipoNotificacao = LEMBRETE_6H`
- [ ] Marca como `Status = PENDENTE` inicialmente
- [ ] Não gera duplicatas (verifica notificação existente do mesmo tipo/ciclo)
- [ ] Registra `DataEnvio` com timestamp

#### Casos de Teste:
1. **Sucesso:** 6h antes do vencimento → Notificação criada
2. **Sucesso:** Lembrete -6h já enviado → Não cria duplicata
3. **Sucesso:** Check-in realizado → Lembrete -6h é cancelado (opcional)

#### Tarefas Técnicas:
- `ServicoAlerta.GerarLembretes6hAsync()`
- `RepositorioNotificacao.ExisteNotificacaoAsync(usuarioId, tipoNotificacao, cicloId)`
- Job registrado em Program.cs com Quartz ou Timer

#### Estimativa: 4h

---

### **US-09: Gerar Alerta de Lembrete (-2h antes do vencimento)**

**Como** Sistema  
**Eu quero** calcular quando faltam 2 horas para vencimento  
**Para que** eu dispare um lembrete urgente ao usuário

#### Regra de Negócio:
- Similar a US-08, mas com -2h
- Dispara sempre (mesmo que -6h não tenha sido enviado)
- Considerado "última chance" para o usuário

#### Critérios de Aceitação:
- [ ] Job verifica a cada 10 minutos
- [ ] Cria `Notificacao` com `TipoNotificacao = LEMBRETE_2H`
- [ ] Não gera duplicatas
- [ ] Se check-in feito, cancela este alerta

#### Casos de Teste:
1. **Sucesso:** 2h antes do vencimento → Notificação criada
2. **Sucesso:** Check-in realizado → Lembrete -2h cancelado

#### Tarefas Técnicas:
- `ServicoAlerta.GerarLembretes2hAsync()`
- Reutilizar lógica de duplicatas de US-08

#### Estimativa: 2h

---

### **US-10: Disparar Notificação de Emergência (após 48h vencidos)**

**Como** Sistema  
**Eu quero** notificar os contatos de emergência quando o prazo vencer  
**Para que** eles saibam que há risco e possam agir

#### Fluxo de Emergência:
1. `DataProximoVencimento` é ultrapassada (agora > vencimento)
2. Sistema cria `Notificacao` com `TipoNotificacao = EMERGENCIA` para cada contato
3. Notifica via EMAIL e WHATSAPP (2 meios por contato)
4. **Repete a cada 6 horas** até que check-in seja feito
5. Máximo de **5 notificações de emergência** por contato (histórico FIFO)

#### Critérios de Aceitação:
- [ ] Primeira notificação disparada imediatamente após vencimento
- [ ] Repetição automática a cada 6h
- [ ] Histórico limitado a 5 por contato
- [ ] Interrompe quando check-in é realizado
- [ ] Log registra quem foi notificado e quando
- [ ] Suporta múltiplos contatos de emergência

#### Casos de Teste:
1. **Sucesso:** Check-in vencido → 2 notificações (EMAIL + WHATSAPP) criadas por contato
2. **Sucesso:** 6h depois → Notificações repetidas (se sem check-in)
3. **Sucesso:** 5 notificações já existem → 6ª é descartada (FIFO)
4. **Sucesso:** Check-in realizado → Notificações interrompem

#### Tarefas Técnicas:
- `ServicoAlerta.VerificarEmergenciasAsync()`
- `ServicoNotificacao.DispararEmergenciaAsync(usuarioId, contatos)`
- `RepositorioNotificacao.ContarNotificacoesEmergenciaAsync(contatoId)`
- Job Quartz/Timer a cada 6h para repetição

#### Estimativa: 8h

---

### **US-11: Registrar Notificações no Histórico (Máximo 5 por tipo)**

**Como** Sistema  
**Eu quero** manter um histórico de notificações de emergência  
**Para que** eu tenha rastreabilidade e limite de recursos

#### Critérios de Aceitação:
- [ ] Cada contato tem histórico máximo de 5 notificações de emergência
- [ ] Política FIFO: entrada mais antiga é removida quando 6ª chega
- [ ] Notificações de lembrete NÃO são limitadas (apenas rastreadas)
- [ ] `Status` muda para ENVIADA/ERRO após tentativa
- [ ] `DataEnvio` registra quando foi criada

#### Casos de Teste:
1. **Sucesso:** Criar 5 notificações → Histórico contém 5
2. **Sucesso:** Criar 6ª → 1ª é deletada, histórico contém 5
3. **Sucesso:** Listar histórico → Retorna 5 mais recentes DESC

#### Tarefas Técnicas:
- Trigger de banco ou lógica em `ServicoNotificacao.CriarNotificacaoAsync()`
- `RepositorioNotificacao.RemoverNotificacaoAntigaAsync(contatoId)` se count > 5

#### Estimativa: 3h

---

### **US-12: Listar Notificações de um Usuário**

**Como** Usuário  
**Eu quero** ver o histórico de alertas que recebi  
**Para que** eu saiba quando fui alertado

#### Critérios de Aceitação:
- [ ] Retorna notificações de todos os contatos do usuário
- [ ] Inclui: `TipoNotificacao`, `MeioNotificacao`, `DataEnvio`, `Status`
- [ ] Ordena por `DataEnvio` DESC
- [ ] Filtra por tipo (opcional): `?tipo=EMERGENCIA`
- [ ] Paginação (10 por página)

#### Casos de Teste:
1. **Sucesso:** Listar notificações → Array com histórico
2. **Filtro:** `?tipo=EMERGENCIA` → Apenas emergências
3. **Vazio:** Sem notificações → Array vazio

#### Tarefas Técnicas:
- `ControladorNotificacao.GetHistorico([FromQuery] string usuarioId, [FromQuery] string? tipo)`
- DTO: `ListarNotificacoesDTO`

#### Estimativa: 2h

---

## 🎯 Épico E05 - API REST

### **US-13: Endpoint POST /usuarios (Cadastro)**

**Como** Usuário da API  
**Eu quero** cadastrar um novo usuário via HTTP  
**Para que** eu possa começar a usar o ProvaVida

#### Especificação de API:
```
POST /api/v1/usuarios
Content-Type: application/json

{
  "nome": "João Silva",
  "email": "joao@example.com",
  "telefone": "11987654321",
  "contatos": [
    {
      "nome": "Maria Silva",
      "email": "maria@example.com",
      "whatsapp": "11987654322"
    }
  ]
}

Response (201 Created):
{
  "id": "uuid-123",
  "nome": "João Silva",
  "email": "joao@example.com",
  "dataCriacao": "2026-01-31T10:00:00Z",
  "dataProximoCheckIn": "2026-02-02T10:00:00Z"
}
```

#### Critérios de Aceitação:
- [ ] Valida dados obrigatórios
- [ ] Retorna 201 Created com Location header
- [ ] Retorna 422 Unprocessable Entity se dados inválidos
- [ ] Retorna 400 Bad Request se sem contatos

#### Estimativa: 3h

---

### **US-14: Endpoint POST /check-ins (Registrar Check-in)**

**Como** Usuário da API  
**Eu quero** fazer check-in via HTTP  
**Para que** estenda meu prazo de 48h

#### Especificação de API:
```
POST /api/v1/check-ins
Authorization: Bearer {token}

{
  "usuarioId": "uuid-123"
}

Response (201 Created):
{
  "checkInId": "uuid-456",
  "dataCheckIn": "2026-01-31T10:30:00Z",
  "dataProximoVencimento": "2026-02-02T10:30:00Z",
  "diasAteVencimento": 2
}
```

#### Critérios de Aceitação:
- [ ] Requer autenticação (Bearer token)
- [ ] Valida usuário existe
- [ ] Retorna 201 se sucesso
- [ ] Retorna 404 se usuário não existe
- [ ] Retorna 401 se sem autenticação

#### Estimativa: 3h

---

### **US-15: Endpoint GET /check-ins/historico (Histórico)**

**Como** Usuário da API  
**Eu quero** listar meus últimos check-ins  
**Para que** veja meu histórico

#### Especificação de API:
```
GET /api/v1/check-ins/historico?usuarioId=uuid-123
Authorization: Bearer {token}

Response (200 OK):
{
  "total": 3,
  "dados": [
    {
      "checkInId": "uuid-456",
      "dataCheckIn": "2026-01-31T10:30:00Z",
      "dataProximoVencimento": "2026-02-02T10:30:00Z",
      "diasAteVencimento": 2,
      "horasAteVencimento": 47
    }
  ]
}
```

#### Estimativa: 2h

---

### **US-16: Endpoint GET /notificacoes/historico (Alertas)**

**Como** Usuário da API  
**Eu quero** ver meu histórico de notificações  
**Para que** saiba quando fui alertado

#### Especificação de API:
```
GET /api/v1/notificacoes/historico?usuarioId=uuid-123&tipo=EMERGENCIA
Authorization: Bearer {token}

Response (200 OK):
{
  "total": 2,
  "dados": [
    {
      "notificacaoId": "uuid-789",
      "tipo": "EMERGENCIA",
      "meio": "WHATSAPP",
      "dataEnvio": "2026-01-31T10:00:00Z",
      "status": "ENVIADA"
    }
  ]
}
```

#### Estimativa: 2h

---

## 📋 Resumo Técnico

| User Story | Épico | Prioridade | Sprint | Estimativa |
|---|---|---|---|---|
| US-01 | E01 | 🔴 | 1 | 2h |
| US-02 | E01 | 🔴 | 1 | 2h |
| US-03 | E01 | 🔴 | 1 | 2h |
| US-04 | E01 | 🔴 | 1 | 2h |
| US-05 | E02 | 🔴 | 2 | 5h |
| US-06 | E03 | 🔴 | 2 | 6h |
| US-07 | E03 | 🟠 | 2 | 3h |
| US-08 | E04 | 🔴 | 3 | 4h |
| US-09 | E04 | 🔴 | 3 | 2h |
| US-10 | E04 | 🔴 | 3 | 8h |
| US-11 | E04 | 🟠 | 3 | 3h |
| US-12 | E04 | 🟠 | 3 | 2h |
| US-13 | E05 | 🟠 | 4 | 3h |
| US-14 | E05 | 🔴 | 4 | 3h |
| US-15 | E05 | 🟠 | 4 | 2h |
| US-16 | E05 | 🟠 | 4 | 2h |

---

## 🔍 Critérios de Aceitação Técnicos

### Todos os Endpoints Devem:
- ✅ Retornar JSON estruturado com `{ "dados": {...}, "erro": null }`
- ✅ Validar input com Data Annotations ou FluentValidation
- ✅ Tratar exceções com middleware global
- ✅ Logar requisições e erros
- ✅ Respeitar autenticação/autorização (se aplicável)

### Todos os Serviços Devem:
- ✅ Usar Result Pattern (`Result<T>`)
- ✅ Ter testes unitários (xUnit)
- ✅ Implementar interfaces do Domínio
- ✅ Comentários em Português descritivos

### Todos os Testes Devem:
- ✅ Usar Arrange-Act-Assert
- ✅ Nomear métodos: `MetodoSob_Condicao_DeveRetornar()`
- ✅ Ter pelo menos 80% de cobertura

---

## 📚 Referências

- [BACKLOG_AGILE.md](BACKLOG_AGILE.md) - Sprints e roadmap
- [ESPECIFICACOES.md](ESPECIFICACOES.md) - Regras de negócio detalhas
- [ARQUITETURA.md](ARQUITETURA.md) - Clean Architecture
- [MODELAGEM.md](MODELAGEM.md) - Entidades e relacionamentos

---

**Próximos Passos:** Iniciar Sprint 1 com US-01 a US-04 (Domínio).
