import React, { useState, useEffect } from "react";
import { StatusCheckIn as StatusCheckInType } from "../../features/dashboard/dashboardService";
import dashboardService from "../../features/dashboard/dashboardService";
import styles from "./StatusCheckIn.module.css";

interface StatusCheckInProps {
  status: StatusCheckInType;
}

const StatusCheckIn: React.FC<StatusCheckInProps> = ({ status }) => {
  const [tempoRestante, setTempoRestante] = useState<string>("");

  useEffect(() => {
    // Atualizar o tempo restante a cada minuto
    const atualizarTempo = () => {
      const tempoFormatado = dashboardService.formatarTempoRestante(
        status.diasRestantes,
        status.horasRestantes,
        status.minutosRestantes
      );
      setTempoRestante(tempoFormatado);
    };

    atualizarTempo();
    const intervalo = setInterval(atualizarTempo, 60000); // Atualizar a cada minuto

    return () => clearInterval(intervalo);
  }, [status]);

  const getCorStatus = () => {
    if (status.vencido) return styles.vencido;
    if (status.diasRestantes === 0 && status.horasRestantes < 6)
      return styles.critico;
    if (status.diasRestantes === 0 && status.horasRestantes < 24)
      return styles.atencao;
    return styles.ok;
  };

  return (
    <div className={`${styles.container} ${getCorStatus()}`}>
      <h3 className={styles.titulo}>Status do Próximo Check-in</h3>

      {status.vencido ? (
        <div className={styles.conteudo}>
          <p className={styles.status}>⚠️ Vencido!</p>
          <p className={styles.urgencia}>
            Realize um check-in imediatamente para evitar alertas aos contatos de emergência.
          </p>
        </div>
      ) : (
        <div className={styles.conteudo}>
          <div className={styles.tempo}>
            <span className={styles.valor}>{tempoRestante}</span>
            <span className={styles.label}>Tempo restante</span>
          </div>
          {status.ultimoCheckInEm && (
            <p className={styles.ultimoCheckIn}>
              Último check-in: {dashboardService.formatarData(status.ultimoCheckInEm)}
            </p>
          )}
        </div>
      )}

      <div className={styles.infoAdicional}>
        <p>Próximo vencimento: {dashboardService.formatarData(status.dataProximoCheckIn)}</p>
      </div>
    </div>
  );
};

export default StatusCheckIn;
