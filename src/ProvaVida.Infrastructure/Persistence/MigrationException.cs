namespace ProvaVida.Infrastructure.Persistence;

public sealed class MigrationException : Exception
{
    public MigrationException(string message, Exception? inner = null)
        : base(message, inner) { }
}
