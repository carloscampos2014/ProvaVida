import React from "react";
import styles from "./BotaoCheckIn.module.css";

interface BotaoCheckInProps {
  onClick: () => void;
  carregando?: boolean;
  vencido?: boolean;
}

const BotaoCheckIn: React.FC<BotaoCheckInProps> = ({
  onClick,
  carregando = false,
  vencido = false,
}) => {
  return (
    <button
      onClick={onClick}
      disabled={carregando}
      className={`${styles.botao} ${vencido ? styles.critico : ""} ${
        carregando ? styles.carregando : ""
      }`}
      aria-label="Fazer check-in"
    >
      {carregando ? (
        <>
          <span className={styles.spinner}></span>
          Enviando...
        </>
      ) : vencido ? (
        <>
          <span className={styles.icon}>⚠️</span>
          Fazer Check-in Agora (Vencido)
        </>
      ) : (
        <>
          <span className={styles.icon}>✓</span>
          Fazer Check-in
        </>
      )}
    </button>
  );
};

export default BotaoCheckIn;
