
## Fluxo de Sincronismo de Dados

### Pré-requisitos

* O App deve ter acesso à Internet

### Fluxo para Sincronizar Dados

1. Inicio
2. O app tem acesso a Internet ?
    * Sim: Ir para o Passo 3
    * Não: Ir para o Passo 10
3. Recuperar dados de sincronismo do servidor
4. Servidor retornou sucesso ?
    * Sim: Ir para o Passo 5 
    * Não: Ir para o Passo 10
5. Atualizar dados de sincronismo no banco local
6. Existem dados pendentes de sincronismo no banco local ?
    * Sim: Ir para o Passo 7 
    * Não: Ir para o Passo 10
7. Enviar dados pendentes de sincronismo ao Servidor
8. Servidor retornou sucesso ?
    * Sim: Marcar dados como sincronizados no banco local
    * Não: Marcar dados como não sincronizados no banco local
9. Atualizar dados de sincronismo no banco local
10. Fim
