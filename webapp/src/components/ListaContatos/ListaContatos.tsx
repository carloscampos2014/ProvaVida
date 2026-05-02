import React from "react";
import { ContatoEmergencia } from "../../features/dashboard/dashboardService";
import styles from "./ListaContatos.module.css";

interface ListaContatosProps {
  contatos: ContatoEmergencia[];
}

const ListaContatos: React.FC<ListaContatosProps> = ({ contatos }) => {
  if (contatos.length === 0) {
    return (
      <div className={styles.vazio}>
        <p>Nenhum contato de emergência cadastrado</p>
        <p className={styles.aviso}>
          ⚠️ É obrigatório ter pelo menos um contato para ativar o monitoramento
        </p>
      </div>
    );
  }

  return (
    <ul className={styles.lista}>
      {contatos.map((contato) => (
        <li
          key={contato.id}
          className={`${styles.item} ${!contato.ativo ? styles.inativo : ""}`}
        >
          <div className={styles.info}>
            <h4 className={styles.nome}>
              {contato.nome}
              {!contato.ativo && <span className={styles.badge}>Inativo</span>}
            </h4>
            <div className={styles.detalhes}>
              <p className={styles.detalhe}>
                <span className={styles.icone}>✉️</span>
                {contato.email}
              </p>
              <p className={styles.detalhe}>
                <span className={styles.icone}>💬</span>
                {contato.whatsApp}
              </p>
            </div>
          </div>
        </li>
      ))}
    </ul>
  );
};

export default ListaContatos;
