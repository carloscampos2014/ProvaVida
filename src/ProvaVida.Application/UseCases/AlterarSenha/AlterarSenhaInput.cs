namespace ProvaVida.Application.UseCases.AlterarSenha;

public record AlterarSenhaInput(Guid UsuarioId, string SenhaAtual, string NovaSenha);
