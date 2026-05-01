import React, { useState, useEffect } from "react";
import { createRoot } from "react-dom/client";
import { LoginForm } from "./features/auth/LoginForm";
import { CadastroForm } from "./features/auth/CadastroForm";
import Dashboard from "./features/dashboard/Dashboard";

type Tela = 'login' | 'cadastro' | 'dashboard';

const App = () => {
  const [tela, setTela] = useState<Tela>('login');
  const [token, setToken] = useState<string | null>(null);

  // Verificar se há token salvo no localStorage
  useEffect(() => {
    const tokenSalvo = localStorage.getItem('token');
    if (tokenSalvo) {
      setToken(tokenSalvo);
      setTela('dashboard');
    }
  }, []);

  const handleLoginSucesso = (novoToken: string) => {
    setToken(novoToken);
    localStorage.setItem('token', novoToken);
    setTela('dashboard');
  };

  const handleLogout = () => {
    setToken(null);
    localStorage.removeItem('token');
    setTela('login');
  };

  const handleCadastroSucesso = () => {
    // Após cadastro bem-sucedido, voltar para login
    setTela('login');
  };

  return (
    <>
      <h1 style={{ display: 'none' }}>ProvaVida WebApp - Sprint 5</h1>
      {tela === "dashboard" && token ? (
        <Dashboard onLogout={handleLogout} />
      ) : tela === "login" ? (
        <LoginForm 
          onSwitchToCadastro={() => setTela("cadastro")}
          onLoginSuccess={handleLoginSucesso}
        />
      ) : (
        <CadastroForm 
          onSwitchToLogin={() => setTela("login")}
          onCadastroSuccess={handleCadastroSucesso}
        />
      )}
    </>
  );
};

const root = createRoot(document.getElementById("root")!);
root.render(<App />);
