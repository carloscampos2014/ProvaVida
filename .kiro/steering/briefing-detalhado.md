---
inclusion: always
---

# Padrão de Briefing Detalhado

Todo briefing de feature ou correção de bug deve incluir:

## Para cada arquivo alterado

- **O que está errado hoje** — comportamento atual com exemplo concreto de código
- **Por que é um problema** — consequência real para o usuário ou sistema
- **O que exatamente vai mudar** — trecho de código antes vs. depois (mesmo que resumido)
- **Por que essa abordagem** — decisão de design em 1 frase

## Formato obrigatório por item

```
**Arquivo: NomeDoArquivo.cs**
Problema: [o que acontece hoje, com código ou fluxo concreto]
Consequência: [o que o usuário ou sistema experimenta]
Correção: [o que muda — antes/depois ou descrição precisa]
Decisão: [por que essa abordagem e não outra]
```

## Nível de detalhe esperado

- Nomear métodos específicos que serão alterados
- Mostrar a assinatura antes e depois quando mudar
- Explicar o mecanismo do bug (race condition, null ref, state compartilhado, etc.)
- Explicar o mecanismo da correção (semáforo, header por request, OnAppearing, etc.)

## O que NÃO fazer

- Listar arquivos sem explicar o que muda em cada um
- Usar termos vagos como "melhora a robustez" sem explicar como
- Omitir o "por quê" — toda decisão precisa de razão
