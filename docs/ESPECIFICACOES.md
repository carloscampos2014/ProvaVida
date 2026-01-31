# Especificações Técnicas e Regras de Negócio

## 1. Regras de Check-in
- **Prazo de Validade:** 48 horas.
- **Extensão:** Cada novo check-in redefine o prazo para as próximas 48 horas.
- **Histórico:** O sistema mantém apenas os últimos 5 registros de check-in (Lógica de Fila/FIFO).

## 2. Regras de Notificação
- **Lembretes:** O sistema deve notificar o usuário quando faltarem 6h e 2h para o prazo expirar.
- **Contatos de Emergência:** - Disparo imediato após o vencimento do prazo.
    - Repetição a cada 6 horas até que o check-in seja realizado.
    - Máximo de 5 notificações de emergência por contato no histórico.

## 3. Requisitos de Conta
- **Obrigatoriedade:** É impossível ativar o monitoramento sem pelo menos um contato de emergência válido (Nome, E-mail e WhatsApp).