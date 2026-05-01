import React from "react";
import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import Dashboard from "../../../webapp/src/features/dashboard/Dashboard";
import dashboardService from "../../../webapp/src/features/dashboard/dashboardService";

// Mock do serviço
jest.mock("../../../webapp/src/features/dashboard/dashboardService");

const mockedDashboardService = dashboardService as jest.Mocked<
  typeof dashboardService
>;

const dadosMockados = {
  usuario: {
    id: "1",
    nome: "João Silva",
    email: "joao@example.com",
    status: 1, // Adicionado para corresponder a UsuarioResumoDto
    dataProximoVencimento: new Date(Date.now() + 48 * 60 * 60 * 1000).toISOString(), // Adicionado
  },
  statusCheckIn: {
    dataProximoCheckIn: new Date(Date.now() + 48 * 60 * 60 * 1000).toISOString(),
    diasRestantes: 1,
    horasRestantes: 23,
    minutosRestantes: 45,
    vencido: false,
    ultimoCheckInEm: new Date().toISOString(),
  },
  contatos: [
    {
      id: "1",
      nome: "Maria Silva",
      email: "maria@example.com",
      whatsapp: "11987654322",
      ativo: true,
    },
  ],
  historico: [
    {
      id: "1",
      dataCheckIn: new Date().toISOString(),
      dataProximoVencimento: new Date(Date.now() + 48 * 60 * 60 * 1000).toISOString(),
      statusNotificacoes: 0,
    },
  ],
  notificacoesPendentes: 0,
};

describe("Dashboard Component", () => {
  beforeEach(() => {
    jest.clearAllMocks();
    mockedDashboardService.obterDadosDashboard.mockResolvedValue(dadosMockados);
  });

  it("Deve renderizar o dashboard com dados carregados", async () => {
    render(<Dashboard />);

    await waitFor(() => {
      expect(screen.getByText("ProvaVida")).toBeInTheDocument();
      expect(screen.getByText(/Bem-vindo, João Silva/)).toBeInTheDocument();
    });
  });

  it("Deve exibir mensagem de carregamento inicialmente", () => {
    mockedDashboardService.obterDadosDashboard.mockImplementationOnce(
      () => new Promise(() => {}) // Never resolves
    );
    render(<Dashboard />);
    expect(screen.getByText("Carregando...")).toBeInTheDocument();
  });

  it("Deve exibir erro ao falhar ao carregar dados", async () => {
    mockedDashboardService.obterDadosDashboard.mockRejectedValueOnce(
      new Error("Erro na API")
    );
    render(<Dashboard />);

    await waitFor(() => {
      expect(
        screen.getByText("Erro ao carregar dados do dashboard")
      ).toBeInTheDocument();
    });
  });

  it("Deve exibir contatos de emergência", async () => {
    render(<Dashboard />);

    await waitFor(() => {
      expect(screen.getByText("Maria Silva")).toBeInTheDocument();
      expect(screen.getByText("maria@example.com")).toBeInTheDocument();
    });
  });

  it("Deve exibir histórico de check-ins", async () => {
    render(<Dashboard />);

    await waitFor(() => {
      expect(screen.getByText("Histórico de Check-ins")).toBeInTheDocument();
    });
  });

  it("Deve chamar fazerCheckIn ao clicar no botão", async () => {
    mockedDashboardService.fazerCheckIn.mockResolvedValueOnce({
      ...dadosMockados.statusCheckIn,
      horasRestantes: 48,
    });

    render(<Dashboard />);

    await waitFor(() => {
      expect(screen.getByText(/Fazer Check-in/i)).toBeInTheDocument();
    });

    const botao = screen.getByText(/Fazer Check-in/i);
    fireEvent.click(botao);

    await waitFor(() => {
      expect(mockedDashboardService.fazerCheckIn).toHaveBeenCalled();
    });
  });

  it("Deve chamar onLogout ao clicar em Sair", async () => {
    const onLogoutMock = jest.fn();
    render(<Dashboard onLogout={onLogoutMock} />);

    await waitFor(() => {
      expect(screen.getByText("Sair")).toBeInTheDocument();
    });

    const botaoSair = screen.getByText("Sair");
    fireEvent.click(botaoSair);

    expect(onLogoutMock).toHaveBeenCalled();
  });

  it("Deve exibir status do check-in", async () => {
    render(<Dashboard />);

    await waitFor(() => {
      expect(screen.getByText("Status do Próximo Check-in")).toBeInTheDocument();
    });
  });

  it("Deve fechar mensagem de erro ao clicar no X", async () => {
    mockedDashboardService.obterDadosDashboard.mockResolvedValueOnce(dadosMockados);
    render(<Dashboard />);

    await waitFor(() => {
      expect(screen.getByText(/Bem-vindo, João Silva/)).toBeInTheDocument();
    });

    // Simular um erro após carregamento
    const botao = screen.getByText(/Fazer Check-in/i);
    mockedDashboardService.fazerCheckIn.mockRejectedValueOnce(
      new Error("Erro ao fazer check-in")
    );
    fireEvent.click(botao);

    await waitFor(() => {
      expect(screen.getByText("Erro ao fazer check-in")).toBeInTheDocument();
    });

    // Fechar erro
    const botaoFechar = screen.getByText("✕");
    fireEvent.click(botaoFechar);

    expect(
      screen.queryByText("Erro ao fazer check-in")
    ).not.toBeInTheDocument();
  });

  it("Deve ter notificações pendentes se houver", async () => {
    const dadosComNotificacoes = {
      ...dadosMockados,
      notificacoesPendentes: 3,
    };
    mockedDashboardService.obterDadosDashboard.mockResolvedValueOnce(
      dadosComNotificacoes
    );

    render(<Dashboard />);

    await waitFor(() => {
      // Verificar se o componente Notificacoes foi renderizado (com o ícone 🔔)
      expect(screen.getByText("🔔")).toBeInTheDocument();
    });
  });
});
