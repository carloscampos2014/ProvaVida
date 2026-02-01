using ProvaVida.Aplicacao.Dtos.ContatosEmergencia;
using ProvaVida.Dominio.Entidades;

namespace ProvaVida.Aplicacao.Mapeadores;

/// <summary>
/// Mapeador manual entre ContatoEmergencia (Entidade) e DTOs.
/// </summary>
public static class ContatoEmergenciaMapeador
{
    /// <summary>
    /// Mapeia ContatoEmergencia (Entidade) para ContatoResumoDto (resposta).
    /// </summary>
    public static ContatoResumoDto ParaResumoDto(this ContatoEmergencia contato)
    {
        if (contato == null)
            throw new ArgumentNullException(nameof(contato), "Contato não pode ser nulo.");

        return new ContatoResumoDto
        {
            Id = contato.Id,
            UsuarioId = contato.UsuarioId,
            Nome = contato.Nome,
            Email = contato.Email.Valor,  // Value Object → string
            WhatsApp = contato.WhatsApp.Valor,
            Prioridade = contato.Prioridade,
            Ativo = contato.Ativo,
            DataCriacao = contato.DataCriacao
        };
    }

    /// <summary>
    /// Mapeia ContatoRegistroDto (entrada) para ContatoEmergencia (Entidade).
    /// 
    /// Factory valida: usuarioId, nome, email, whatsapp, prioridade.
    /// </summary>
    public static ContatoEmergencia ParaDominio(this ContatoRegistroDto dto, Guid usuarioId)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "DTO de contato não pode ser nulo.");

        if (usuarioId == Guid.Empty)
            throw new ArgumentException("ID do usuário é obrigatório.", nameof(usuarioId));

        return ContatoEmergencia.Criar(
            usuarioId: usuarioId,
            nome: dto.Nome,
            email: dto.Email,
            whatsapp: dto.WhatsApp,
            prioridade: dto.Prioridade ?? 1
        );
    }

    /// <summary>
    /// Converte lista de ContatosEmergencia para DTOs.
    /// Utilitário para respostas em massa.
    /// </summary>
    public static List<ContatoResumoDto> ParaResumosDtos(
        this IEnumerable<ContatoEmergencia> contatos)
    {
        if (contatos == null)
            throw new ArgumentNullException(nameof(contatos));

        return contatos
            .Select(c => c.ParaResumoDto())
            .ToList();
    }
}
