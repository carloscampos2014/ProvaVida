namespace ProvaVida.Application.UseCases.RefreshToken;

public record RefreshTokenOutput(
    string Token,
    DateTime ExpiraEm,
    string RefreshToken,
    DateTime RefreshTokenExpiraEm);
