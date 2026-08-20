# Skill: Spec-Driven Development

Desenvolvimento guiado por especificação formal antes de qualquer implementação.

## Quando usar

Ativar para: feature com 3+ arquivos, mudança irreversível, trabalho > 4h, novo contrato de API.
Não usar para: bugfix simples (1-2 arquivos), ajuste de configuração.

## Estrutura

```
.kiro/specs/<nome>/
├── requirements.md   ← user stories + critérios de aceite
├── design.md         ← arquitetura + fluxos + decisões
└── tasks.md          ← tarefas priorizadas com estimativas
```

## Processo

1. Elicitar requisitos → requirements.md → **aguardar aprovação**
2. Design técnico → design.md → **aguardar aprovação**
3. Quebrar em tasks → tasks.md
4. Implementar task por task (build + testes em cada)
5. Validar critérios de aceite ao final

**Regra:** Nunca implementar sem requirements e design aprovados pelo usuário.

## User Story

```
Como [usuário], quero [ação], para [valor].

Critérios de aceite:
- CA-01: [Dado/Quando/Então]
Fora do escopo: [o que NÃO faz]
```
