using ProvaVida.Dominio.Enums;
using ProvaVida.Dominio.Exceções;
using ProvaVida.Dominio.ObjetosValor;

namespace ProvaVida.Dominio.Entidades;

/// <summary>
/// Entidade de Domínio que representa um usuário no sistema ProvaVida.
/// Um usuário é a pessoa que será monitorada e que fará os check-ins periódicos.
/// 
/// Invariantes:
/// - Um usuário DEVE ter pelo menos um contato de emergência.
/// - O prazo próximo check-in é sempre DataUltimoCheckIn + 48 horas.
/// - Um usuário não pode estar em dois estados simultaneamente.
/// </summary>
public sealed class Usuario
{
    private readonly List<ContatoEmergencia> _contatos = new();
    private readonly List<CheckIn> _historicoCheckIns = new();

    /// <summary>
    /// Identificador único do usuário (GUID).
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Nome completo do usuário.
    /// </summary>
    public string Nome { get; private set; } = null!;

    /// <summary>
    /// Email do usuário (Value Object com validação).
    /// </summary>
    public Email Email { get; private set; } = null!;

    /// <summary>
    /// Telefone do usuário para contato (Value Object com validação).
    /// </summary>
    public Telefone Telefone { get; private set; } = null!;

    /// <summary>
    /// Hash da senha para autenticação.
    /// </summary>
    public string SenhaHash { get; private set; } = null!;

    /// <summary>
    /// Data e hora do último check-in realizado pelo usuário.
    /// </summary>
    public DateTime DataUltimoCheckIn { get; private set; }

    /// <summary>
    /// Data e hora do próximo vencimento (sempre DataUltimoCheckIn + 48 horas).
    /// Calculado automaticamente, não deve ser alterado diretamente.
    /// </summary>
    public DateTime DataProximoVencimento { get; private set; }

    /// <summary>
    /// Status atual do usuário (Ativo, EmAtraso, AlertaCritico).
    /// </summary>
    public StatusUsuario Status { get; private set; }

    /// <summary>
    /// Data de criação do usuário no sistema.
    /// </summary>
    public DateTime DataCriacao { get; private set; }

    /// <summary>
    /// Data da última atualização do usuário.
    /// </summary>
    public DateTime DataUltimaAtualizacao { get; private set; }

    /// <summary>
    /// Coleção de contatos de emergência associados a este usuário (somente leitura).
    /// </summary>
    public IReadOnlyCollection<ContatoEmergencia> Contatos => _contatos.AsReadOnly();

    /// <summary>
    /// Coleção de histórico de check-ins (limitada a 5 registros mais recentes).
    /// </summary>
    public IReadOnlyCollection<CheckIn> HistoricoCheckIns => _historicoCheckIns.AsReadOnly();

    private Usuario() { }

    /// <summary>
    /// Factory para criar um novo usuário com os dados obrigatórios.
    /// </summary>
    /// <param name="nome">Nome do usuário.</param>
    /// <param name="email">Email do usuário (será validado).</param>
    /// <param name="telefone">Telefone do usuário (será validado).</param>
    /// <param name="senhaHash">Hash da senha.</param>
    /// <returns>Novo usuário com o estado inicial configurado.</returns>
    /// <exception cref="UsuarioInvalidoException">Se algum parâmetro for inválido.</exception>
    public static Usuario Criar(string nome, string email, string telefone, string senhaHash)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new UsuarioInvalidoException("Nome do usuário não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new UsuarioInvalidoException("Senha não pode ser vazia.");

        try
        {
            var usuarioEmail = new Email(email);
            var usuarioTelefone = new Telefone(telefone);

            var agora = DateTime.UtcNow;

            return new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = nome.Trim(),
                Email = usuarioEmail,
                Telefone = usuarioTelefone,
                SenhaHash = senhaHash,
                DataUltimoCheckIn = agora,
                DataProximoVencimento = agora.AddHours(48),
                Status = StatusUsuario.Ativo,
                DataCriacao = agora,
                DataUltimaAtualizacao = agora
            };
        }
        catch (ArgumentException ex)
        {
            throw new UsuarioInvalidoException($"Erro ao validar dados do usuário: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Adiciona um contato de emergência ao usuário.
    /// </summary>
    /// <param name="contato">O contato a ser adicionado.</param>
    /// <exception cref="ArgumentNullException">Se o contato for nulo.</exception>
    public void AdicionarContato(ContatoEmergencia contato)
    {
        if (contato == null)
            throw new ArgumentNullException(nameof(contato));

        if (contato.UsuarioId != Id)
            throw new InvalidOperationException("O contato não pertence a este usuário.");

        if (_contatos.Any(c => c.Email == contato.Email))
            throw new InvalidOperationException("Já existe um contato com este email.");

        _contatos.Add(contato);
        DataUltimaAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Remove um contato de emergência do usuário.
    /// </summary>
    /// <param name="contatoId">O ID do contato a remover.</param>
    /// <exception cref="ContatoObrigatorioException">Se tentar remover o último contato.</exception>
    public void RemoverContato(Guid contatoId)
    {
        if (_contatos.Count <= 1)
            throw new ContatoObrigatorioException("Você deve manter pelo menos um contato de emergência.");

        var contato = _contatos.FirstOrDefault(c => c.Id == contatoId);
        if (contato != null)
        {
            _contatos.Remove(contato);
            DataUltimaAtualizacao = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Registra um novo check-in, resetando o prazo de 48 horas.
    /// </summary>
    /// <exception cref="CheckInInvalidoException">Se o check-in não puder ser registrado.</exception>
    public void RegistrarCheckIn()
    {
        var agora = DateTime.UtcNow;
        DataUltimoCheckIn = agora;
        DataProximoVencimento = agora.AddHours(48);
        Status = StatusUsuario.Ativo;
        DataUltimaAtualizacao = agora;

        // Adiciona ao histórico
        var checkIn = new CheckIn(Guid.NewGuid(), Id, agora, DataProximoVencimento);
        _historicoCheckIns.Add(checkIn);

        // Mantém apenas os 5 registros mais recentes
        LimparHistoricoExcedente();
    }

    /// <summary>
    /// Calcula se o prazo de 48 horas foi ultrapassado.
    /// </summary>
    /// <returns>True se agora > DataProximoVencimento.</returns>
    public bool EstaVencido()
    {
        return DateTime.UtcNow > DataProximoVencimento;
    }

    /// <summary>
    /// Calcula quantas horas faltam para o vencimento do prazo.
    /// </summary>
    /// <returns>Número de horas (pode ser negativo se vencido).</returns>
    public double HorasAteVencimento()
    {
        return (DataProximoVencimento - DateTime.UtcNow).TotalHours;
    }

    /// <summary>
    /// Verifica se um lembrete de -6 horas deve ser disparado.
    /// </summary>
    /// <returns>True se faltam entre 6h e 5h59m para vencimento.</returns>
    public bool DeveDispararLembrete6h()
    {
        var horasAte = HorasAteVencimento();
        return horasAte > 0 && horasAte <= 6;
    }

    /// <summary>
    /// Verifica se um lembrete de -2 horas deve ser disparado.
    /// </summary>
    /// <returns>True se faltam entre 2h e 1h59m para vencimento.</returns>
    public bool DeveDispararLembrete2h()
    {
        var horasAte = HorasAteVencimento();
        return horasAte > 0 && horasAte <= 2;
    }

    /// <summary>
    /// Atualiza o status do usuário baseado no tempo decorrido.
    /// </summary>
    public void AtualizarStatus()
    {
        if (EstaVencido())
        {
            Status = StatusUsuario.AlertaCritico;
        }
        else if (HorasAteVencimento() < 6)
        {
            Status = StatusUsuario.EmAtraso;
        }
        else
        {
            Status = StatusUsuario.Ativo;
        }

        DataUltimaAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Valida se o usuário pode continuar usando o sistema.
    /// Retorna false se não possui nenhum contato de emergência.
    /// </summary>
    /// <returns>True se o usuário está válido e ativo.</returns>
    public bool EstaValido()
    {
        return Status != StatusUsuario.Inativo && _contatos.Count > 0;
    }

    /// <summary>
    /// Remove check-ins antigos, mantendo apenas os 5 mais recentes (FIFO).
    /// </summary>
    private void LimparHistoricoExcedente()
    {
        const int limiteRegistros = 5;

        if (_historicoCheckIns.Count > limiteRegistros)
        {
            var registrosParaRemover = _historicoCheckIns
                .OrderBy(c => c.DataCheckIn)
                .Take(_historicoCheckIns.Count - limiteRegistros)
                .ToList();

            foreach (var registro in registrosParaRemover)
            {
                _historicoCheckIns.Remove(registro);
            }
        }
    }
}
