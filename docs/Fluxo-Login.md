
## Fluxo de Login

### Pré-requisitos

* O App deve ter acesso à Internet

### Fluxo para Login

1. Inicio
2. O app tem acesso a Internet ?
    * Sim: Ir para o Passo 6
    * Não: Ir para o Passo 3
3. Exibir mensagem de erro "Não é possivel efetuar login sem acesso a internet."
4. Encerrar App
5. Ir para o Passo 15
6. Campos obrigatórios preenchidos ?
    * Sim: Ir para o Passo 9 
    * Não: Ir para o Passo 7
7. Exibir mensagem de erro "Preencha todos os campos obrigatórios."
8. Ir para o Passo 15
9. Enviar pedido de login ao Servidor
10. Servidor retornou sucesso ?
    * Sim: Ir para o Passo 14
    * Não: Ir para o Passo 11
11. Exibir mensagem de erro "Login ou senha inválidos."
12. Ir para o Passo 15
13. Incluir dados do usuario no banco de dados local
14. Abrir tela de Checkin
15. Fim
