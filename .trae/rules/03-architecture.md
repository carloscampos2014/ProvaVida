# Regra: Arquitetura

---

## Clean Architecture — Regra de Dependência (inviolável)

```
Domain        ← ZERO dependências externas
    ↑
Application   ← depende apenas de Domain; interfaces aqui
    ↑
Infrastructure← implementa interfaces de Application
    ↑
Api           ← controllers finos, delega para Application
```

**Violações proibidas:**
- `Domain` referenciar `Infrastructure`, `Api`, EF Core, ASP.NET Core
- `Application` referenciar `Infrastructure`, `DbContext`, `Api`
- Controllers acessar banco diretamente

## Nomenclatura de projetos

```
NomeProjeto.Domain
NomeProjeto.Application
NomeProjeto.Infrastructure
NomeProjeto.Api
NomeProjeto.Shared
```

## Controllers

- Thin — receber request, delegar para handler/use case, retornar resposta
- Sem lógica de negócio
- Sem acesso direto ao banco

## Use Cases / Handlers

- Um use case = uma classe com uma responsabilidade
- Retornar `Result<T>` — nunca `void` ou tipo HTTP
- Validação antes de qualquer efeito colateral
