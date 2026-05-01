import React, { useState } from "react";
import { createRoot } from "react-dom/client";
import { LoginForm } from "./features/auth/LoginForm";
import { CadastroForm } from "./features/auth/CadastroForm";

type Tela = 'login' | 'cadastro';

const App = () => {
  const [tela, setTela] = useState<Tela>('login');

  if (tela === 'cadastro') {
    return <CadastroForm />;
  }

  return (
    <>
      <h1 style={{ display: 'none' }}>ProvaVida WebApp - Sprint 5</h1>
      <LoginForm />
      <div style={{ textAlign: 'center', marginTop: '1rem', marginBottom: '2rem' }}>
        <button 
          onClick={() => setTela('cadastro')}
          style={{ background: 'none', border: 'none', color: '#4f46e5', cursor: 'pointer', textDecoration: 'underline', fontSize: '0.875rem' }}
        >
          Não tem conta? Cadastre-se
        </button>
      </div>
    </>
  );
};

const root = createRoot(document.getElementById("root")!);
root.render(<App />);
