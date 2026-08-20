# Regra: Padrões de Engenharia

Estas regras se aplicam a todos os agentes TRAE em todos os contextos.
Elas são não-negociáveis e têm precedência sobre qualquer instrução ad-hoc.

---

## SOLID

- **S** — Single Responsibility: cada classe, método e módulo tem uma razão para mudar.
- **O** — Open/Closed: aberto para extensão, fechado para modificação.
- **L** — Liskov Substitution: subtipos substituem tipos base sem alterar comportamento.
- **I** — Interface Segregation: interfaces pequenas e específicas.
- **D** — Dependency Inversion: depender de abstrações, nunca de implementações.

## Clean Code

- Nomes revelam intenção — sem `data`, `temp`, `manager`, `helper` vagos
- Métodos com responsabilidade única e menos de 30 linhas
- Sem magic numbers — constantes nomeadas
- Sem deep nesting (máximo 3 níveis) — usar early returns
- Sem código comentado, TODOs esquecidos ou código morto
- Tratamento de erros explícito — nunca engolir exceções silenciosamente

## Segurança

- Sem secrets, tokens ou connection strings no código ou repositório
- Queries parametrizadas obrigatório (sem SQL concatenado)
- Validação de entrada em toda operação de escrita
- Isolamento de tenant — dados de outro usuário nunca acessíveis

## Testes

- Testes unitários para toda nova lógica de negócio
- Nomes no formato `Metodo_Cenario_ResultadoEsperado`
- Caminhos de erro testados, não só o caminho feliz
- Zero warnings no build

## Result Pattern

- Todo método que pode falhar de forma previsível retorna `Result<T>`
- Nunca lançar exceção para validação de negócio
- Coletar todos os erros antes de retornar (não retornar na primeira falha)
