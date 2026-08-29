
## Fluxo de Heartbeat

### Pré-requisitos

* O App deve ter acesso à Internet

### Fluxo para Heartbeat

1. Inicio
2. O app tem acesso a Internet ?
    * Sim: Ir para o Passo 3
    * Não: Ir para o Passo 6
3. Enviar pedido de heartbeat ao Servidor
4. Servidor retornou sucesso ?
    * Sim: Ir para o Passo 5 
    * Não: Ir para o Passo 6
5. Incluir dados de heartbeat no banco local
6. Fim
