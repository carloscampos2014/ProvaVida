import React from "react";
import styles from "./Notificacoes.module.css";

interface NotificacoesProps {
  quantidade: number;
}

const Notificacoes: React.FC<NotificacoesProps> = ({ quantidade }) => {
  if (quantidade === 0) {
    return null;
  }

  return (
    <div className={styles.container} title={`${quantidade} notificações pendentes`}>
      <span className={styles.icone}>🔔</span>
      <span className={styles.badge}>{quantidade > 9 ? "9+" : quantidade}</span>
    </div>
  );
};

export default Notificacoes;
