# Arquitetura do Projeto - Comum Prova de Vida

Este projeto utiliza os princípios da **Clean Architecture** para garantir testabilidade, manutenção e independência tecnológica.

## 🏗️ Estrutura de Camadas

### 1. Camada de Domínio (Dominio)
- **Papel:** O núcleo do sistema. Contém a lógica que não muda.
- **Conteúdo:** Entidades, Enums, Interfaces de Repositório e Exceções de Negócio.
- **Regra:** Não depende de nenhuma outra camada.

### 2. Camada de Aplicação (Aplicacao)
- **Papel:** Orquestra o fluxo de dados. Implementa os "Casos de Uso".
- **Conteúdo:** Serviços, DTOs (Data Transfer Objects), Mapeamentos e Validadores.
- **Exemplos:** `ServicoCheckIn.cs`, `ServicoNotificacao.cs`.
- **Regra:** Depende apenas da camada de Domínio.

### 3. Camada de Infraestrutura (Infraestrutura)
- **Papel:** Detalhes de implementação e ferramentas externas.
- **Conteúdo:** Contexto do Banco de Dados (EF Core), Implementação de Repositórios, Integração com APIs de terceiros (WhatsApp/E-mail).
- **Regra:** Implementa as interfaces definidas no Domínio.

### 4. Camada de API / Apresentação (API)
- **Papel:** Porta de entrada do sistema.
- **Conteúdo:** Controllers, Filtros, Configurações de Dependency Injection.

---

## 🛠️ Padrões e Princípios Adotados

- **SOLID:** Cada classe tem uma única responsabilidade.
- **Injeção de Dependência:** Utilizada nativamente pelo .NET para desacoplar as camadas.
- **Result Pattern:** Os serviços retornam um objeto de sucesso ou falha, evitando o uso excessivo de exceções para controle de fluxo.
- **TDD (Test Driven Development):** A lógica de cálculo do prazo de 48h deve ser coberta por testes unitários antes da implementação.

## 🔄 Fluxo de um Check-in
1. A **API** recebe a requisição.
2. A **Aplicação** valida o usuário e chama o **Domínio**.
3. O **Domínio** atualiza as datas e valida as regras de negócio.
4. A **Infraestrutura** persiste os dados no banco e limpa registros antigos (mantendo apenas 
5. Estrutura de Pastas e Responsabilidades (Detalhado)
    ProvaVida/
    ├── src/
    │   ├── ProvaVida.Dominio/          # O Coração (Entidades e Regras)
    │   ├── ProvaVida.Aplicacao/        # Casos de Uso (Serviços e DTOs)
    │   ├── ProvaVida.Infraestrutura/   # Ferramentas (Banco de Dados e APIs)
    │   └── ProvaVida.API/              # Entrada (Controllers)
    ├── docs/                           # Documentação (.md)
    └── tests/                          # Testes Unitários
