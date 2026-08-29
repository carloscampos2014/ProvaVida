## Fluxo de Excluir Conta

### Pré-requisitos

* O App deve ter acesso à Internet

### Fluxo para Excluir Conta

1. Inicio
2. O app tem acesso a Internet ?
    * Sim: Ir para o Passo 5
    * Não: Ir para o Passo 3
3. Exibir mensagem de erro "Não é possivel efetuar exclusão de conta sem acesso a internet."
4. Ir para o Passo 14
5. Exibir mensagem de confirmação "Tem certeza que deseja excluir sua conta?"
    * Sim: Ir para o Passo 6
    * Não: Ir para o Passo 14
6. Enviar pedido de exclusão de conta ao Servidor
7. Servidor retornou sucesso ?
    * Sim: Ir para o Passo 10
    * Não: Ir para o Passo 8
8. Exibir mensagem de erro recebida do servidor
9. Ir para o Passo 14
10. Excluir dados de checkins do banco local
11. Excluir dados do usuário do banco local
12. Exibir mensagem de sucesso "Conta excluída com sucesso!"
13. Abrir tela de Login
14. Fim
