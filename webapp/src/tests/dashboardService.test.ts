import dashboardService, {
  DadosDashboard,
  StatusCheckIn,
  UsuarioResumoDto,
  ContatoResumoDto,
  CheckInResumoDto,
  CheckInRegistroDto,
} from "../../../webapp/src/features/dashboard/dashboardService";
import api from "../../../webapp/src/services/api";

// Mock do axios
jest.mock("../../../webapp/src/services/api");
jest.spyOn(localStorage, 'getItem').mockReturnValue('test-user-id-123');

const mockedApi = api as jest.Mocked<typeof api>;

const statusMockado: StatusCheckIn = {
  dataProximoCheckIn: new Date(Date.now() + 48 * 60 * 60 * 1000).toISOString(),
  diasRestantes: 1,
  horasRestantes: 23,
  minutosRestantes: 45, // Adicionado
  ultimoCheckInEm: new Date().toISOString(),
};

const usuarioResumoMock: UsuarioResumoDto = { // Completado o mock
  id: "test-user-id-123",
  nome: "João Silva",
  email: "joao@example.com",
  status: 1,
  dataProximoVencimento: new Date(Date.now() + 48 * 60 * 60 * 1000).toISOString(),
  quantidadeContatos: 1,
  dataCriacao: new Date().toISOString(),
};

const contatosResumoMock: ContatoResumoDto[] = [
  {
    id: "contact-id-1",
    usuarioId: "test-user-id-123",
    nome: "Maria Silva",
    email: "maria@example.com",
    whatsApp: "11987654322",
    prioridade: 1,
    ativo: true,
    dataCriacao: new Date().toISOString(),
  },
];

const historicoCheckInMock: CheckInResumoDto[] = [
  {
    id: "checkin-id-1",
    usuarioId: "test-user-id-123",
    dataCheckIn: new Date().toISOString(),
    dataProximoVencimento: new Date(Date.now() + 48 * 60 * 60 * 1000).toISOString(), // Completado
    mensagem: "Check-in realizado com sucesso", // Adicionado
  },
];

const notificacoesPendentesMock = 0; // Adicionado

const fullDadosDashboardMock: DadosDashboard = { // Adicionado para consistência, embora não usado diretamente no teste atual
  usuario: usuarioResumoMock,
  statusCheckIn: statusMockado,
  contatos: contatosResumoMock,
  historico: historicoCheckInMock,
  notificacoesPendentes: notificacoesPendentesMock,
};

describe("DashboardService", () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe("obterDadosDashboard", () => {
    it("Deve fazer requisições GET corretas e agregar os dados", async () => {
      mockedApi.get
        .mockResolvedValueOnce({ data: { dados: usuarioResumoMock } }) // User profile
        .mockResolvedValueOnce({ data: { dados: contatosResumoMock } }) // Contacts
        .mockResolvedValueOnce({ data: { dados: historicoCheckInMock } }) // History
        .mockResolvedValueOnce({ data: { total: notificacoesPendentesMock } }); // Notifications

      const result = await dashboardService.obterDadosDashboard();

      expect(mockedApi.get).toHaveBeenCalledWith("/api/v1/Auth/test-user-id-123");
      expect(mockedApi.get).toHaveBeenCalledWith("/api/v1/Contatos/test-user-id-123");
      expect(mockedApi.get).toHaveBeenCalledWith("/api/v1/CheckIns/historico/test-user-id-123");
      expect(mockedApi.get).toHaveBeenCalledWith("/api/notificacoes/pendentes");

      expect(result.usuario).toEqual(usuarioResumoMock);
      expect(result.contatos).toEqual(contatosResumoMock);
      expect(result.historico).toEqual(historicoCheckInMock);
      expect(result.notificacoesPendentes).toEqual(notificacoesPendentesMock);
      // Check derived statusCheckIn fields
      expect(result.statusCheckIn.dataProximoCheckIn).toEqual(usuarioResumoMock.dataProximoVencimento);
      expect(result.statusCheckIn.ultimoCheckInEm).toEqual(historicoCheckInMock[0].dataCheckIn);
    });

    it("Deve lançar erro ao falhar na requisição", async () => {
      mockedApi.get.mockRejectedValueOnce(new Error("Erro de rede"));
      await expect(
        dashboardService.obterDadosDashboard()
      ).rejects.toThrow("Erro de rede");
    });
  });

  describe("fazerCheckIn", () => {
    it("Deve fazer POST para realizar check-in e retornar o status atualizado", async () => {
      const newCheckInResponse: CheckInResumoDto = {
        id: "new-checkin-id",
        usuarioId: "test-user-id-123",
        dataCheckIn: new Date().toISOString(),
        dataProximoVencimento: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString(), // 7 days from now
        mensagem: "Check-in successful",
      };
      mockedApi.post.mockResolvedValueOnce({ data: { dados: newCheckInResponse } }); // Completado

      const result = await dashboardService.fazerCheckIn(localizacaoMock);

      const expectedCheckInRegistro: CheckInRegistroDto = {
        usuarioId: "test-user-id-123",
        localizacao: localizacaoMock,
      };

      expect(mockedApi.post).toHaveBeenCalledWith("/api/v1/CheckIns/registrar", expectedCheckInRegistro);
      expect(result.dataProximoCheckIn).toEqual(newCheckInResponse.dataProximoVencimento);
      expect(result.ultimoCheckInEm).toEqual(newCheckInResponse.dataCheckIn);
      expect(result.vencido).toBeFalsy();
    });

    it("Deve lançar erro ao falhar no check-in", async () => {
      mockedApi.post.mockRejectedValueOnce(new Error("Check-in falhou"));

      await expect(dashboardService.fazerCheckIn("Some Location")).rejects.toThrow(
        "Check-in falhou"
      );
    });
  });

  describe("obterContatos", () => { // Corrigido o nome do describe
    it("Deve fazer requisição GET dos contatos", async () => { // Completado
      const result = await dashboardService.obterContatos();

      expect(mockedApi.get).toHaveBeenCalledWith("/api/v1/Contatos/test-user-id-123");
      expect(result).toEqual(contatosResumoMock);
    });
  });

  describe("obterHistoricoCheckIns", () => {
    it("Deve fazer requisição GET do histórico", async () => { // Completado
      mockedApi.get.mockResolvedValueOnce({ data: { dados: historicoCheckInMock } }); // Completado

      const result = await dashboardService.obterHistoricoCheckIns();

      expect(mockedApi.get).toHaveBeenCalledWith("/api/v1/CheckIns/historico/test-user-id-123");
      expect(result).toEqual(historicoCheckInMock); // Completado
    });
  });

  describe("obterNotificacoesPendentes", () => {
    it("Deve fazer requisição GET das notificações", async () => {
      mockedApi.get.mockResolvedValueOnce({ data: { total: 3 } }); // Assuming this endpoint still returns { total: number }

      const result = await dashboardService.obterNotificacoesPendentes();

      expect(mockedApi.get).toHaveBeenCalledWith("/api/notificacoes/pendentes");
      expect(result).toBe(3);
    });

    it("Deve retornar 0 em caso de erro", async () => {
      mockedApi.get.mockRejectedValueOnce(new Error("Erro de API"));
      const result = await dashboardService.obterNotificacoesPendentes();
      expect(result).toBe(0);
    });
  });

  describe("formatarTempoRestante", () => {
    it("Deve formatar com dias, horas e minutos", () => {
      const resultado = dashboardService.formatarTempoRestante(1, 5, 30);
      expect(resultado).toBe("1d 5h 30m");
    });

    it("Deve formatar tempo apenas em minutos", () => {
      const resultado = dashboardService.formatarTempoRestante(0, 0, 45);
      expect(resultado).toBe("45m");
    });
  });

  describe("formatarData", () => {
    it("Deve formatar data no padrão brasileiro", () => {
      const data = "2026-05-01T20:00:00Z";
      const resultado = dashboardService.formatarData(data);
      expect(resultado).toMatch(/\d{2}\/\d{2}\/\d{4}/);
    });
  });
});
