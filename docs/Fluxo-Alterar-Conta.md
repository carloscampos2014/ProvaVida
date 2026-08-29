
## Fluxo de Alterar Conta

### Pré-requisitos

* O App deve ter acesso à Internet

### Fluxo para Alterar Conta

1. Inicio
2. O app tem acesso a Internet ?
    * Sim: Ir para o Passo 5
    * Não: Ir para o Passo 3
3. Exibir mensagem de erro "Não é possivel efetuar alteração de conta sem acesso a internet."
4. Ir para o Passo 13
5. Campos obrigatórios preenchidos ?
    * Sim: Ir para o Passo 8 
    * Não: Ir para o Passo 6
6. Exibir mensagem de erro "Preencha todos os campos obrigatórios."
7. Ir para o Passo 14
8. Enviar pedido de alteração ao Servidor
9. Servidor retornou sucesso ?
    * Sim: Ir para o Passo 12
    * Não: Ir para o Passo 10
10. Exibir mensagem de erro recebida do servidor
11. Ir para o Passo 14
12. Exibir mensagem de sucesso "Alteração realizada com sucesso!"
13. Abrir tela de Login
14. Fim
