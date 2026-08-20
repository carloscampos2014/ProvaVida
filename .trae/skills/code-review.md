# Skill: Code Review

Revisão estruturada cobrindo arquitetura, qualidade, segurança e testes.

## Quando usar

Ativar quando: revisar PR, analisar código antes de aprovar, verificar conformidade.

## Checklist

**Arquitetura**
- [ ] Dependency Rule respeitada
- [ ] Controller thin, sem lógica de negócio
- [ ] Result Pattern aplicado
- [ ] Interfaces em Application, implementações em Infrastructure

**Clean Code**
- [ ] Nomes revelam intenção
- [ ] Métodos < 30 linhas
- [ ] Sem magic numbers
- [ ] Nesting máximo 3 níveis

**Segurança**
- [ ] Sem secrets inline
- [ ] Queries parametrizadas
- [ ] Validação de entrada
- [ ] Isolamento de tenant

**Testes**
- [ ] Novos comportamentos cobertos
- [ ] Nomes `Metodo_Cenario_Resultado`
- [ ] Caminhos de erro testados

## Severidades

- 🔴 Crítico — bloquear (segurança, dado incorreto, build quebrado)
- 🟡 Atenção — corrigir antes do merge
- 🔵 Sugestão — melhoria futura
- ✅ Elogio — reconhecer boas decisões

## Formato de saída

```
📋 Code Review — [arquivo/PR]
🔴 Crítico (N): [arquivo:linha] — [problema] | [correção]
🟡 Atenção (N): [arquivo:linha] — [problema]
✅ Pontos positivos: [lista]
Veredicto: ✅ Aprovado / 🟡 Com ressalvas / 🔴 Requer correções
```
