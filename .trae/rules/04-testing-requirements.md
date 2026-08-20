# Regra: Requisitos de Testes

Estas regras se aplicam a todos os agentes TRAE para qualquer tarefa que envolva código.

---

## Cobertura mínima

| Camada | Cobertura mínima |
|--------|-----------------|
| Domain (regras de negócio) | 95%+ |
| Application (use cases, services) | 80%+ |
| Infrastructure | 60%+ |
| Api / Controllers | 70%+ |

## Nomenclatura obrigatória

Formato: `Metodo_Cenario_ResultadoEsperado`

```
✅ CriarPedido_ItensVazios_RetornaErroDeValidacao
✅ Login_CredenciaisInvalidas_RetornaUnauthorized
✅ CalcularDesconto_ClienteVip_Aplica20Porcento

❌ TesteCriarPedido
❌ Deve_Funcionar
❌ Teste1
```

## Estrutura AAA obrigatória

```csharp
[Fact]
public async Task CriarPedido_ItensVazios_RetornaErro()
{
    // Arrange
    var handler = new CriarPedidoHandler(Substitute.For<IPedidoRepository>());
    var command = new CriarPedidoCommand(ClienteId: "c1", Itens: []);

    // Act
    var result = await handler.HandleAsync(command);

    // Assert
    result.Success.Should().BeFalse();
    result.Errors.Should().Contain("Pedido deve ter ao menos um item");
}
```

## Cenários obrigatórios

Para cada critério de aceite, cobrir:
- Caminho feliz (entrada válida, resultado esperado)
- Caminho de falha (entrada inválida, erro esperado)
- Boundary values quando aplicável (0, 1, máximo, máximo+1)

## Proibições

```
❌ Testes que apenas verificam que "não lança exceção"
❌ Mocks que sempre retornam sucesso sem testar falha
❌ Testes dependentes de ordem de execução
❌ Thread.Sleep / Task.Delay em testes
❌ Assert vago sem mensagem de erro clara
```
