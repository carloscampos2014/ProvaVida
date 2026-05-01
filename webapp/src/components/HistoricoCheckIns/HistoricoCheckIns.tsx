import React from "react";
import { HistoricoCheckIn } from "../../features/dashboard/dashboardService";
import dashboardService from "../../features/dashboard/dashboardService";
import styles from "./HistoricoCheckIns.module.css";

interface HistoricoCheckInsProps {
  historico: HistoricoCheckIn[];
}

const HistoricoCheckIns: React.FC<HistoricoCheckInsProps> = ({ historico }) => {
  if (historico.length === 0) {
    return (
      <div className={styles.vazio}>
        <p>Nenhum check-in registrado ainda</p>
      </div>
    );
  }

  return (
    <div className={styles.container}>
      <table className={styles.tabela}>
        <thead>
          <tr>
            <th>Data do Check-in</th>
            <th>Próximo Vencimento</th>
            <th>Notificações</th>
          </tr>
        </thead>
        <tbody>
          {historico.map((item) => (
            <tr key={item.id}>
              <td>{dashboardService.formatarData(item.dataCheckIn)}</td>
              <td>{dashboardService.formatarData(item.dataProximoVencimento)}</td>
              <td>
                <span className={styles.badge}>{item.statusNotificacoes}</span>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      <p className={styles.nota}>
        Mostrando os últimos 5 check-ins (mantemos apenas os 5 mais recentes)
      </p>
    </div>
  );
};

export default HistoricoCheckIns;
