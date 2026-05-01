import api from "../../services/api";

// --- Updated Interfaces based on OpenAPI Spec ---

export interface UsuarioResumoDto { // Renamed from UsuarioDashboard to match OpenAPI
  id: string;
  nome: string;
  email: string;
  status: number; // Assuming StatusUsuario enum maps to a number
  dataProximoVencimento: string;
  quantidadeContatos: number;
  dataCriacao: string;
  // Removed 'telefone' and 'ativo' as they are not directly in UsuarioResumoDto
  // If 'telefone' is needed, it should be fetched from another endpoint or added to UsuarioResumoDto in backend.
  // 'ativo' might be inferred from 'status' enum.
}

// This interface is derived on the frontend, not directly from API
export interface StatusCheckIn {
  dataProximoCheckIn: string;
  diasRestantes: number;
  horasRestantes: number;
  minutosRestantes: number;
  vencido: boolean;
  ultimoCheckInEm?: string;
}

export interface ContatoResumoDto { // Renamed from ContatoEmergencia to match OpenAPI
  id: string;
  usuarioId: string;
  nome: string;
  email: string;
  whatsApp: string; // Changed from 'whatsapp' to 'whatsApp'
  prioridade: number;
  ativo: boolean;
  dataCriacao: string;
}

export interface CheckInResumoDto { // Renamed from HistoricoCheckIn to match OpenAPI
  id: string;
  usuarioId: string;
  dataCheckIn: string;
  dataProximoVencimento: string;
  mensagem?: string;
  // Removed 'statusNotificacoes' as it's not in CheckInResumoDto
}

export interface CheckInRegistroDto {
  usuarioId: string;
  localizacao?: string;
}

// Response DTO for API calls that return a single item with message and timestamp
interface ApiResponse<T> {
  dados: T;
  mensagem?: string;
  timestamp: string;
}

// Response DTO for API calls that return a list of items with message and timestamp
interface ApiListResponse<T> {
  dados: T[];
  mensagem?: string;
  timestamp: string;
}

export interface DadosDashboard {
  usuario: UsuarioResumoDto; // Updated type
  statusCheckIn: StatusCheckIn;
  contatos: ContatoResumoDto[]; // Updated type
  historico: CheckInResumoDto[]; // Updated type
  notificacoesPendentes: number;
}

class DashboardService {
  private getUserId(): string {
    // In a real application, this would come from an authentication context or similar.
    // For now, we'll assume it's stored in localStorage after login.
    const userId = localStorage.getItem("userId");
    if (!userId) {
      // Handle case where userId is not available, e.g., redirect to login
      throw new Error("User ID not found. Please log in.");
    }
    return userId;
  }

  /**
   * Obter todos os dados do dashboard do usuário
   */
  async obterDadosDashboard(): Promise<DadosDashboard> {
    const userId = this.getUserId();
    try {
      const [usuarioRes, contatosRes, historicoRes, notificacoes] = await Promise.all([
        api.get<UsuarioResumoDtoApiResponse>(`/api/v1/Auth/${userId}`), // Corrigido o tipo de resposta esperado
        this.obterContatos(),
        this.obterHistoricoCheckIns(),
        this.obterNotificacoesPendentes()
      ]);

      // Tenta pegar de .dados (se seguir o DTO de resposta) ou usa o corpo diretamente
      const usuario = usuarioRes.data?.dados || (usuarioRes.data as unknown as UsuarioResumoDto);
      const contatos = contatosRes; // Removido .data.dados, pois obterContatos já retorna o array
      const historico = historicoRes; // Removido .data.dados, pois obterHistoricoCheckIns já retorna o array

      if (!usuario) {
        throw new Error("Não foi possível carregar os dados do usuário.");
      }

      // Derive statusCheckIn from fetched data
      const statusCheckIn = this.derivarStatusCheckIn(usuario, historico);

      return {
        usuario,
        statusCheckIn,
        contatos,
        historico,
        notificacoesPendentes: notificacoes
      };
    } catch (erro) {
      console.error("Erro ao agregar dados do dashboard:", erro);
      throw erro;
    }
  }

  /**
   * Realizar check-in (prova de vida)
   */
  async fazerCheckIn(localizacao?: string): Promise<StatusCheckIn> {
    const userId = this.getUserId();
    try {
      const checkInRegistro: CheckInRegistroDto = {
        usuarioId: userId,
        localizacao: localizacao, // Pass location if available
      };
      const response = await api.post<ApiResponse<CheckInResumoDto>>("/api/v1/CheckIns/registrar", checkInRegistro);
      const novoCheckIn = response.data.dados;

      // After a successful check-in, we need to re-fetch or update the dashboard data
      // For simplicity, we'll just return a derived status based on the new check-in date
      // In a real app, you might want to trigger a full dashboard reload or update the state more intelligently.
      const now = new Date();
      const proximoVencimento = new Date(novoCheckIn.dataProximoVencimento);
      const diffMs = proximoVencimento.getTime() - now.getTime();
      const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));
      const diffHours = Math.floor((diffMs % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
      const diffMinutes = Math.floor((diffMs % (1000 * 60 * 60)) / (1000 * 60));

      return {
        dataProximoCheckIn: novoCheckIn.dataProximoVencimento,
        diasRestantes: diffDays,
        horasRestantes: diffHours,
        minutosRestantes: diffMinutes,
        vencido: diffMs <= 0,
        ultimoCheckInEm: novoCheckIn.dataCheckIn,
      };

    } catch (erro) {
      console.error("Erro ao fazer check-in:", erro);
      throw erro;
    }
  }

  /**
   * Derivar status atual do check-in a partir dos dados do usuário e histórico.
   */
  private derivarStatusCheckIn(usuario: UsuarioResumoDto, historico: CheckInResumoDto[]): StatusCheckIn {
    const proximoVencimentoStr = usuario?.dataProximoVencimento;
    const proximoVencimento = proximoVencimentoStr ? new Date(proximoVencimentoStr) : new Date(NaN);

    // Verificação defensiva para evitar o TypeError caso o usuário seja undefined
    if (!usuario || isNaN(proximoVencimento.getTime())) {
      return {
        dataProximoCheckIn: "",
        diasRestantes: 0,
        horasRestantes: 0,
        minutosRestantes: 0,
        vencido: false
      };
    }

    const now = new Date();
    const diffMs = proximoVencimento.getTime() - now.getTime();

    const diasRestantes = Math.floor(diffMs / (1000 * 60 * 60 * 24));
    const horasRestantes = Math.floor((diffMs % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
    const minutosRestantes = Math.floor((diffMs % (1000 * 60 * 60)) / (1000 * 60));
    const vencido = diffMs <= 0;

    const ultimoCheckIn = historico.length > 0
      ? historico.sort((a, b) => new Date(b.dataCheckIn).getTime() - new Date(a.dataCheckIn).getTime())[0].dataCheckIn
      : undefined;

    return {
      dataProximoCheckIn: usuario.dataProximoVencimento,
      diasRestantes,
      horasRestantes,
      minutosRestantes,
      vencido,
      ultimoCheckInEm: ultimoCheckIn,
    };
  }

  /**
   * Obter lista de contatos de emergência
   */
  async obterContatos(): Promise<ContatoResumoDto[]> {
    const userId = this.getUserId();
    try {
      const response = await api.get<ApiListResponse<ContatoResumoDto>>(
        `/api/v1/Contatos/${userId}`
      );
      return response.data.dados;
    } catch (erro) {
      console.error("Erro ao obter contatos:", erro);
      throw erro;
    }
  }

  /**
   * Obter histórico de check-ins (últimos 5)
   */
  async obterHistoricoCheckIns(): Promise<CheckInResumoDto[]> {
    const userId = this.getUserId();
    try {
      const response = await api.get<ApiListResponse<CheckInResumoDto>>(`/api/v1/CheckIns/historico/${userId}`);
      return response.data.dados;
    } catch (erro) {
      console.error("Erro ao obter histórico:", erro);
      throw erro;
    }
  }

  /**
   * Contar notificações pendentes
   */
  // NOTE: This endpoint is not explicitly defined in the provided OpenAPI spec.
  // Assuming it still exists or needs to be implemented on the backend.
  async obterNotificacoesPendentes(): Promise<number> {
    // Como este endpoint não existe no seu backend atual (OpenAPI),
    // retornamos 0 diretamente para evitar o erro 404 no console e no log.
    return 0;
  }

  /**
   * Formatar o tempo restante em string legível
   */
  formatarTempoRestante(
    dias: number,
    horas: number,
    minutos: number
  ): string {
    if (dias > 0) {
      return `${dias}d ${horas}h`;
    }
    if (horas > 0) {
      return `${horas}h ${minutos}m`;
    }
    return `${minutos}m`;
  }

  /**
   * Formatar data para exibição
   */
  formatarData(data: string): string {
    if (!data) return "--/--/----";
    
    const dataObj = new Date(data);
    if (isNaN(dataObj.getTime())) return "Data inválida";

    return dataObj.toLocaleDateString("pt-BR", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  }
}

export default new DashboardService();
