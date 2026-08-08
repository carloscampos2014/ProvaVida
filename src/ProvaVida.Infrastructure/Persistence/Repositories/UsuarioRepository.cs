using Dapper;
using ProvaVida.Application.Interfaces;
using ProvaVida.Domain.Entities;

namespace ProvaVida.Infrastructure.Persistence.Repositories;

/// <summary>
/// Leituras usam DbConnectionFactory diretamente (sem transação).
/// Escritas usam IUnitOfWork (com transação aberta pelo caso de uso).
/// </summary>
public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly IUnitOfWork _uow;
    private readonly DbConnectionFactory _factory;

    public UsuarioRepository(IUnitOfWork uow, DbConnectionFactory factory)
    {
        _uow = uow;
        _factory = factory;
    }

    public async Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id                          AS "Id",
                   nome                        AS "Nome",
                   email                       AS "Email",
                   whatsapp                    AS "WhatsApp",
                   senha_hash                  AS "SenhaHash",
                   ativo                       AS "Ativo",
                   contato_emergencia_nome     AS "ContatoEmergenciaNome",
                   contato_emergencia_email    AS "ContatoEmergenciaEmail",
                   contato_emergencia_whatsapp AS "ContatoEmergenciaWhatsApp",
                   criado_em                   AS "CriadoEm",
                   atualizado_em               AS "AtualizadoEm"
            FROM usuarios
            WHERE id = @Id
            LIMIT 1
            """;

        using var conn = _factory.CreateConnection();
        var row = await conn.QueryFirstOrDefaultAsync<UsuarioRow>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));

        return row is null ? null : Mapear(row);
    }

    public async Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id                          AS "Id",
                   nome                        AS "Nome",
                   email                       AS "Email",
                   whatsapp                    AS "WhatsApp",
                   senha_hash                  AS "SenhaHash",
                   ativo                       AS "Ativo",
                   contato_emergencia_nome     AS "ContatoEmergenciaNome",
                   contato_emergencia_email    AS "ContatoEmergenciaEmail",
                   contato_emergencia_whatsapp AS "ContatoEmergenciaWhatsApp",
                   criado_em                   AS "CriadoEm",
                   atualizado_em               AS "AtualizadoEm"
            FROM usuarios
            WHERE LOWER(email) = LOWER(@Email)
            LIMIT 1
            """;

        using var conn = _factory.CreateConnection();
        var row = await conn.QueryFirstOrDefaultAsync<UsuarioRow>(
            new CommandDefinition(sql, new { Email = email }, cancellationToken: ct));

        return row is null ? null : Mapear(row);
    }

    public async Task<bool> EmailExisteAsync(string email, CancellationToken ct = default)
    {
        const string sql = "SELECT COUNT(1) FROM usuarios WHERE LOWER(email) = LOWER(@Email)";

        using var conn = _factory.CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { Email = email }, cancellationToken: ct));

        return count > 0;
    }

    public async Task AdicionarAsync(Usuario usuario, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO usuarios (
                id, nome, email, whatsapp, senha_hash, ativo,
                contato_emergencia_nome, contato_emergencia_email, contato_emergencia_whatsapp,
                criado_em, atualizado_em
            ) VALUES (
                @Id, @Nome, @Email, @WhatsApp, @SenhaHash, @Ativo,
                @ContatoEmergenciaNome, @ContatoEmergenciaEmail, @ContatoEmergenciaWhatsApp,
                @CriadoEm, @AtualizadoEm
            )
            """;

        await _uow.Connection.ExecuteAsync(
            new CommandDefinition(sql, new
            {
                usuario.Id,
                usuario.Nome,
                usuario.Email,
                usuario.WhatsApp,
                usuario.SenhaHash,
                usuario.Ativo,
                usuario.ContatoEmergenciaNome,
                usuario.ContatoEmergenciaEmail,
                usuario.ContatoEmergenciaWhatsApp,
                usuario.CriadoEm,
                usuario.AtualizadoEm
            }, transaction: _uow.Transaction, cancellationToken: ct));
    }

    public async Task AtualizarAsync(Usuario usuario, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE usuarios SET
                nome                        = @Nome,
                whatsapp                    = @WhatsApp,
                contato_emergencia_nome     = @ContatoEmergenciaNome,
                contato_emergencia_email    = @ContatoEmergenciaEmail,
                contato_emergencia_whatsapp = @ContatoEmergenciaWhatsApp,
                atualizado_em               = @AtualizadoEm
            WHERE id = @Id
            """;

        await _uow.Connection.ExecuteAsync(
            new CommandDefinition(sql, new
            {
                usuario.Id,
                usuario.Nome,
                usuario.WhatsApp,
                usuario.ContatoEmergenciaNome,
                usuario.ContatoEmergenciaEmail,
                usuario.ContatoEmergenciaWhatsApp,
                usuario.AtualizadoEm
            }, transaction: _uow.Transaction, cancellationToken: ct));
    }

    public async Task AnonimizarAsync(
        Guid id, string nomeSubstituto, string emailSubstituto, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE usuarios SET
                nome                        = @Nome,
                email                       = @Email,
                whatsapp                    = '[removido]',
                senha_hash                  = '',
                ativo                       = FALSE,
                contato_emergencia_nome     = '[removido]',
                contato_emergencia_email    = '[removido]',
                contato_emergencia_whatsapp = '[removido]',
                atualizado_em               = NOW()
            WHERE id = @Id
            """;

        await _uow.Connection.ExecuteAsync(
            new CommandDefinition(sql,
                new { Id = id, Nome = nomeSubstituto, Email = emailSubstituto },
                transaction: _uow.Transaction, cancellationToken: ct));
    }

    public async Task InvalidarSessoesAsync(Guid usuarioId, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE sessoes_login SET ativo = FALSE
            WHERE usuario_id = @UsuarioId AND ativo = TRUE
            """;

        await _uow.Connection.ExecuteAsync(
            new CommandDefinition(sql, new { UsuarioId = usuarioId },
                transaction: _uow.Transaction, cancellationToken: ct));
    }

    public Task SalvarAlteracoesAsync(CancellationToken ct = default)
        => Task.CompletedTask; // commit via IUnitOfWork.CommitAsync()

    // ----- Mapeamento (contorna private setters do Domain) -----

#pragma warning disable CA1812
    private sealed class UsuarioRow
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string WhatsApp { get; set; } = string.Empty;
        public string SenhaHash { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public string ContatoEmergenciaNome { get; set; } = string.Empty;
        public string ContatoEmergenciaEmail { get; set; } = string.Empty;
        public string ContatoEmergenciaWhatsApp { get; set; } = string.Empty;
        public DateTime CriadoEm { get; set; }
        public DateTime AtualizadoEm { get; set; }
    }
#pragma warning restore CA1812

    private static Usuario Mapear(UsuarioRow row)
    {
        var u = (Usuario)System.Runtime.CompilerServices
            .RuntimeHelpers.GetUninitializedObject(typeof(Usuario));

        Set(u, nameof(Usuario.Id), row.Id);
        Set(u, nameof(Usuario.Nome), row.Nome);
        Set(u, nameof(Usuario.Email), row.Email);
        Set(u, nameof(Usuario.WhatsApp), row.WhatsApp);
        Set(u, nameof(Usuario.SenhaHash), row.SenhaHash);
        Set(u, nameof(Usuario.Ativo), row.Ativo);
        Set(u, nameof(Usuario.ContatoEmergenciaNome), row.ContatoEmergenciaNome);
        Set(u, nameof(Usuario.ContatoEmergenciaEmail), row.ContatoEmergenciaEmail);
        Set(u, nameof(Usuario.ContatoEmergenciaWhatsApp), row.ContatoEmergenciaWhatsApp);
        Set(u, nameof(Usuario.CriadoEm), row.CriadoEm);
        Set(u, nameof(Usuario.AtualizadoEm), row.AtualizadoEm);

        return u;
    }

    private static void Set(object obj, string prop, object? valor)
    {
        typeof(Usuario)
            .GetProperty(prop,
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance)
            ?.SetValue(obj, valor);
    }
}
