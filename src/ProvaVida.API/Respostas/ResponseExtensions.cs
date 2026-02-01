using Microsoft.AspNetCore.Mvc;

namespace ProvaVida.API.Respostas;

/// <summary>
/// Extensões para facilitar criação de respostas padronizadas.
/// </summary>
public static class ResponseExtensions
{
    /// <summary>
    /// Cria uma resposta de sucesso genérica (OK 200).
    /// </summary>
    public static IActionResult Ok<T>(this ControllerBase controller, T dados, string? mensagem = null)
    {
        return controller.Ok(new ApiResponse<T>
        {
            Dados = dados,
            Mensagem = mensagem ?? "✅ Operação realizada com sucesso.",
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Cria uma resposta de criação (201 Created).
    /// </summary>
    public static IActionResult Created<T>(
        this ControllerBase controller, 
        string routeName, 
        T dados, 
        object? routeValues = null)
    {
        return controller.CreatedAtAction(routeName, routeValues, new ApiResponse<T>
        {
            Dados = dados,
            Mensagem = "✅ Recurso criado com sucesso.",
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Cria uma resposta de erro customizada.
    /// </summary>
    public static IActionResult BadRequest<T>(this ControllerBase controller, string mensagem)
    {
        return controller.BadRequest(new ErrorResponse
        {
            Titulo = "Erro na Requisição",
            Mensagem = mensagem,
            StatusCode = StatusCodes.Status400BadRequest,
            Timestamp = DateTime.UtcNow
        });
    }
}
