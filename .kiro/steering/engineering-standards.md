---
inclusion: auto
---

# Padrões de Engenharia de Software

Todo código produzido deve seguir obrigatoriamente os princípios abaixo. Eles não são opcionais e se aplicam a qualquer tarefa de implementação, independente do tamanho.

## SOLID

- **S** — Single Responsibility: cada classe, método e módulo tem uma única razão para mudar.
- **O** — Open/Closed: aberto para extensão, fechado para modificação.
- **L** — Liskov Substitution: subtipos são substituíveis por seus tipos base sem alterar o comportamento.
- **I** — Interface Segregation: interfaces pequenas e específicas; nunca forçar implementação de métodos não usados.
- **D** — Dependency Inversion: depender de abstrações, nunca de implementações concretas.

## Clean Code

- Nomes revelam intenção: variáveis, métodos, funções e classes têm nomes que dispensam comentário.
- Métodos/funções fazem uma coisa só e são curtos.
- Sem números mágicos — usar constantes nomeadas.
- Sem comentários que explicam "o quê" — o código deve ser autoexplicativo. Comentários explicam "por quê".
- Sem código morto, sem TODOs esquecidos, sem código comentado.
- Tratamento de erros explícito — nunca engolir exceções/erros silenciosamente.

## Separação de responsabilidades

- Backend (.NET): controllers finos — recebem request, delegam para serviços/casos de uso, retornam resposta. Lógica de negócio fica fora dos controllers.
- App mobile: componentes de UI não contêm lógica de negócio nem chamadas diretas à API — delegar para hooks/serviços.
- Serviços de infraestrutura (e-mail, WhatsApp, SQLite) acessados via interfaces/abstrações, nunca diretamente pelo domínio.

## Testes

- Testes unitários cobrem lógica de negócio e serviços de aplicação.
- Testes de integração usam banco real via Testcontainers (backend).
- Nomes descritivos no formato: `Metodo_Cenario_ResultadoEsperado`.
- Sem testes que apenas verificam que "não lança exceção" — testar comportamento real.

## Segurança

- Sem credenciais, segredos ou connection strings no código ou repositório.
- Validação de entrada em toda operação de escrita (FluentValidation no backend; validação nos formulários do app).
- Senhas sempre com hash (bcrypt/argon2) — nunca em texto plano.
- Comunicação cliente-servidor exclusivamente via HTTPS.
- Token JWT armazenado de forma segura no dispositivo (keychain/keystore).

## Regras gerais

- Warnings tratados como erros no build do backend — o build não aceita warnings.
- Sem estado global mutável compartilhado entre requisições na API.
- Código estruturado para facilitar evolução futura (ex.: separar banco de dados da VM, escalar horizontalmente) sem redesenho completo.
