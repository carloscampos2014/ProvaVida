import React, { useState } from "react";
import { createRoot } from "react-dom/client";
import { LoginForm } from "./features/auth/LoginForm";
import { CadastroForm } from "./features/auth/CadastroForm";

type Tela = 'login' | 'cadastro';

const App = () => {
  const [tela, setTela] = useState<Tela>('login');

  return (
    <>
      <h1 style={{ display: 'none' }}>ProvaVida WebApp - Sprint 5</h1>
      {tela === "login" ? (
        <LoginForm onSwitchToCadastro={() => setTela("cadastro")} />
      ) : (
        <CadastroForm onSwitchToLogin={() => setTela("login")} />
      )}
    </>
  );
};

const root = createRoot(document.getElementById("root")!);
root.render(<App />);
