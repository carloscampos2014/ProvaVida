# Testes de Integração - Prioridade SQLite

- Todos os testes críticos de integração utilizam SQLite (in-memory ou arquivo), garantindo compatibilidade total com CI/CD (ex: GitHub Actions).
- Testes com PostgreSQL (Testcontainers) são opcionais e marcados com [Trait("Categoria", "Opcional-Postgres")]. Execute-os apenas em ambiente local com Docker disponível.
- No pipeline CI, apenas os testes SQLite são executados por padrão.

## Recomendações
- Para novos testes de integração, utilize sempre SQLite.
- Para rodar todos os testes localmente, inclua os opcionais conforme necessidade.

---

Dúvidas? Consulte o time de QA ou a documentação do projeto.
