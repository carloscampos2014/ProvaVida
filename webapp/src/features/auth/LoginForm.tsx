import React, { useState } from "react";
import { loginUsuario } from "./authService"; // Assuming authService is updated
import { Button } from "../../components/Button";

const styles = {
  container: {
    minHeight: '100vh',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    background: 'linear-gradient(135deg, #1a1a2e 0%, #16213e 100%)',
    padding: '1rem',
  },
  card: {
    background: '#fff',
    borderRadius: '16px',
    padding: '2.5rem',
    width: '100%',
    maxWidth: '400px',
    boxShadow: '0 25px 50px -12px rgba(0, 0, 0, 0.25)',
  },
  logo: {
    textAlign: 'center' as const,
    marginBottom: '2rem',
  },
  title: {
    fontSize: '1.5rem',
    fontWeight: '700',
    color: '#1a1a2e',
    marginBottom: '0.5rem',
    textAlign: 'center' as const,
  },
  subtitle: {
    color: '#6b7280',
    textAlign: 'center' as const,
    marginBottom: '2rem',
    fontSize: '0.875rem',
  },
  inputGroup: {
    marginBottom: '1.25rem',
  },
  label: {
    display: 'block',
    fontSize: '0.875rem',
    fontWeight: '500',
    color: '#374151',
    marginBottom: '0.5rem',
  },
  input: {
    width: '100%',
    padding: '0.75rem 1rem',
    border: '2px solid #e5e7eb',
    borderRadius: '8px',
    fontSize: '1rem',
    transition: 'border-color 0.2s, box-shadow 0.2s',
    outline: 'none',
    boxSizing: 'border-box' as const,
  },
  error: {
    color: '#dc2626',
    fontSize: '0.875rem',
    marginTop: '0.5rem',
    display: 'flex',
    alignItems: 'center',
    gap: '0.5rem',
  },
  footer: {
    textAlign: 'center' as const,
    marginTop: '1.5rem',
    color: '#6b7280',
    fontSize: '0.875rem',
  },
  linkRow: {
    textAlign: 'center' as const,
    marginTop: '1rem',
    fontSize: '0.9rem',
    color: '#6b7280',
  },
  link: {
    background: 'none',
    border: 'none',
    color: '#4f46e5',
    fontWeight: 600,
    cursor: 'pointer',
    textDecoration: 'underline',
    fontSize: '0.9rem',
  },
};

interface LoginFormProps {
  onSwitchToCadastro?: () => void;
  onLoginSuccess?: (token: string) => void;
}

export const LoginForm: React.FC<LoginFormProps> = ({ onSwitchToCadastro, onLoginSuccess }) => {
  const [email, setEmail] = useState("");
  const [senha, setSenha] = useState("");
  const [erro, setErro] = useState("");
  const [carregando, setCarregando] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setCarregando(true);
    setErro("");
    try {
      const response: UsuarioResumoDtoApiResponse = await loginUsuario({ email, senha });
      const userId = response.dados.id; // Get user ID from the response
      localStorage.setItem("userId", userId); // Store userId for future API calls
      // Assuming the actual authentication token is handled by the API client (e.g., Axios interceptor)
      onLoginSuccess?.(userId); // Pass userId as the "token" for now, or adjust onLoginSuccess to expect userId
    } catch (err) {
      setErro("Login inválido. Verifique suas credenciais.");
    } finally {
      setCarregando(false);
    }
  };

  return (
    <div style={styles.container}>
      <div style={styles.card}>
        <div style={styles.logo}>
          <div style={{ fontSize: '3rem', marginBottom: '0.5rem' }}>🛡️</div>
          <div style={styles.title}>ProvaVida</div>
          <div style={styles.subtitle}>Sistema de Segurança Pessoal</div>
        </div>

        <form onSubmit={handleSubmit}>
          <div style={styles.inputGroup}>
            <label style={styles.label}>E-mail</label>
            <input
              type="email"
              placeholder="seu@email.com"
              value={email}
              onChange={e => setEmail(e.target.value)}
              required
              style={styles.input}
            />
          </div>

          <div style={styles.inputGroup}>
            <label style={styles.label}>Senha</label>
            <input
              type="password"
              placeholder="••••••••"
              value={senha}
              onChange={e => setSenha(e.target.value)}
              required
              style={styles.input}
            />
          </div>

          {erro && (
            <div style={styles.error}>
              ⚠️ {erro}
            </div>
          )}

          <Button 
            type="submit" 
            disabled={carregando}
            style={{ width: '100%', marginTop: '1rem', padding: '0.875rem' }}
          >
            {carregando ? "Entrando..." : "Entrar"}
          </Button>
        </form>

        <div style={styles.footer}>
          <p>🔒 Seus dados estão protegidos</p>
        </div>

        {onSwitchToCadastro && (
          <div style={styles.linkRow}>
            Não tem conta?{" "}
            <button type="button" onClick={onSwitchToCadastro} style={styles.link}>
              Cadastre-se
            </button>
          </div>
        )}
      </div>
    </div>
  );
};
