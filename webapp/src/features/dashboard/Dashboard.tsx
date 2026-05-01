import React, { useState, useEffect } from "react";
import dashboardService, { DadosDashboard } from "./dashboardService";
import { BotaoCheckIn, ListaContatos, HistoricoCheckIns, Notificacoes, StatusCheckIn } from "../../components";
import styles from "./Dashboard.module.css";

interface DashboardProps {
  onLogout?: () => void;
}

const Dashboard: React.FC<DashboardProps> = ({ onLogout }) => {
  const [dados, setDados] = useState<DadosDashboard | null>(null);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState<string | null>(null);
  const [fazendoCheckIn, setFazendoCheckIn] = useState(false);

  useEffect(() => {
    carregarDados();
  }, []);

  const carregarDados = async () => {
    try {
      setCarregando(true);
      setErro(null);
      const dadosDashboard = await dashboardService.obterDadosDashboard();
      setDados(dadosDashboard);
    } catch (err) {
      setErro("Erro ao carregar dados do dashboard");
      console.error(err);
    } finally {
      setCarregando(false);
    }
  };

  const handleCheckIn = async () => {
    try {
      setFazendoCheckIn(true);
      const novoStatus = await dashboardService.fazerCheckIn(); // Assuming location is not required or handled internally
      
      // Atualizar apenas o status do check-in
      if (dados) {
        setDados({
          ...dados,
          statusCheckIn: novoStatus,
        });
      }
    } catch (err) {
      setErro("Erro ao fazer check-in");
      console.error(err);
    } finally {
      setFazendoCheckIn(false);
    }
  };

  if (carregando) {
    return (
      <div className={styles.container}>
        <p className={styles.carregando}>Carregando...</p>
      </div>
    );
  }

  if (erro && !dados) {
    return (
      <div className={styles.container}>
        <div className={styles.erro}>
          <p>{erro}</p>
          <button onClick={carregarDados} className={styles.botaoRetry}>
            Tentar Novamente
          </button>
        </div>
      </div>
    );
  }

  if (!dados) {
    return (
      <div className={styles.container}>
        <p className={styles.carregando}>Sem dados disponíveis</p>
      </div>
    );
  }

  return (
    <div className={styles.container}>
      {/* Header */}
      <header className={styles.header}>
        <div className={styles.headerContent}>
          <h1 className={styles.titulo}>ProvaVida</h1>
          <div className={styles.headerAcoes}>
            <Notificacoes quantidade={dados.notificacoesPendentes} />
            {onLogout && (
              <button onClick={onLogout} className={styles.botaoLogout}>
                Sair
              </button>
            )}
          </div>
        </div>
      </header>

      {/* Conteúdo Principal */}
      <main className={styles.main}>
        {/* Saudação */}
        <section className={styles.saudacao}>
          <h2>Bem-vindo, {dados.usuario.nome}!</h2>
          <p>Mantenha-se conectado com suas provas de vida</p>
        </section>

        {/* Status e Check-in */}
        <section className={styles.secaoStatus}>
          <StatusCheckIn status={dados.statusCheckIn} />
          <BotaoCheckIn
            onClick={handleCheckIn}
            carregando={fazendoCheckIn}
            vencido={dados.statusCheckIn.vencido}
          />
        </section>

        {/* Grid de Conteúdo */}
        <div className={styles.grid}>
          {/* Contatos */}
          <section className={styles.card}>
            <h3>Contatos de Emergência</h3>
            <ListaContatos contatos={dados.contatos} />
          </section>

          {/* Histórico */}
          <section className={styles.card}>
            <h3>Histórico de Check-ins</h3>
            <HistoricoCheckIns historico={dados.historico} />
          </section>
        </div>

        {/* Mensagem de Erro */}
        {erro && (
          <div className={styles.erroAlert}>
            <p>{erro}</p>
            <button onClick={() => setErro(null)} className={styles.botaoFechar}>
              ✕
            </button>
          </div>
        )}
      </main>
    </div>
  );
};

export default Dashboard;
