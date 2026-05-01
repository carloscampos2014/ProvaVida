import React, { useState } from "react";
import { cadastrarUsuario } from "./authService";

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
    padding: '2rem',
    width: '100%',
    maxWidth: '450px',
    boxShadow: '0 25px 50px -12px rgba(0, 0, 0, 0.25)',
  },
  header: {
    textAlign: 'center' as const,
    marginBottom: '1.5rem',
  },
  title: {
    fontSize: '1.5rem',
    fontWeight: '700',
    color: '#1a1a2e',
    marginBottom: '0.5rem',
  },
  subtitle: {
    color: '#6b7280',
    fontSize: '0.875rem',
  },
  sectionTitle: {
    fontSize: '0.75rem',
    fontWeight: '600',
    color: '#4f46e5',
    textTransform: 'uppercase' as const,
    letterSpacing: '0.05em',
    marginBottom: '0.75rem',
    marginTop: '1.5rem',
  },
  inputGroup: {
    marginBottom: '1rem',
  },
  label: {
    display: 'block',
    fontSize: '0.875rem',
    fontWeight: '500',
    color: '#374151',
    marginBottom: '0.375rem',
  },
  input: {
    width: '100%',
    padding: '0.625rem 0.875rem',
    border: '2px solid #e5e7eb',
    borderRadius: '8px',
    fontSize: '0.9375rem',
    outline: 'none',
    boxSizing: 'border-box' as const,
    transition: 'border-color 0.2s',
  },
  row: {
    display: 'grid',
    gridTemplateColumns: '1fr 1fr',
    gap: '1rem',
  },
  error: {
    color: '#dc2626',
    fontSize: '0.8125rem',
    marginTop: '0.375rem',
  },
  success: {
    color: '#059669',
    fontSize: '0.875rem',
    padding: '0.75rem',
    background: '#d1fae5',
    borderRadius: '8px',
    marginBottom: '1rem',
  },
  button: {
    width: '100%',
    padding: '0.875rem',
    background: '#4f46e5',
    color: '#fff',
    border: 'none',
    borderRadius: '8px',
    fontSize: '1rem',
    fontWeight: '600',
    cursor: 'pointer',
    marginTop: '1.5rem',
    transition: 'opacity 0.2s',
  },
  footer: {
    textAlign: 'center' as const,
    marginTop: '1.5rem',
    fontSize: '0.875rem',
    color: '#6b7280',
  },
  link: {
    color: '#4f46e5',
    cursor: 'pointer',
    textDecoration: 'underline',
  },
};

interface ContatoEmergencia {
  nome: string;
  email: string;
  whatsapp: string;
}

export const CadastroForm: React.FC = () => {
  const [etapa, setEtapa] = useState<'dados' | 'contato'>('dados');
  
  // Dados do usuário
  const [nome, setNome] = useState("");
  const [email, setEmail] = useState("");
  const [telefone, setTelefone] = useState("");
  const [senha, setSenha] = useState("");
  const [confirmarSenha, setConfirmarSenha] = useState("");
  
  // Contato de emergência
  const [contatoNome, setContatoNome] = useState("");
  const [contatoEmail, setContatoEmail] = useState("");
  const [contatoWhatsapp, setContatoWhatsapp] = useState("");
  
  const [erro, setErro] = useState("");
  const [sucesso, setSucesso] = useState("");
  const [carregando, setCarregando] = useState(false);

  const validarDados = (): boolean => {
    if (!nome.trim()) {
      setErro("Nome é obrigatório");
      return false;
    }
    if (!email.includes("@")) {
      setErro("Email inválido");
      return false;
    }
    if (senha.length < 6) {
      setErro("Senha deve ter pelo menos 6 caracteres");
      return false;
    }
    if (senha !== confirmarSenha) {
      setErro("As senhas não conferem");
      return false;
    }
    return true;
  };

  const validarContato = (): boolean => {
    if (!contatoNome.trim()) {
      setErro("Nome do contato de emergência é obrigatório");
      return false;
    }
    if (!contatoEmail.includes("@")) {
      setErro("Email do contato inválido");
      return false;
    }
    if (!contatoWhatsapp.trim()) {
      setErro("WhatsApp do contato é obrigatório");
      return false;
    }
    return true;
  };

  const handleContinuar = (e: React.FormEvent) => {
    e.preventDefault();
    setErro("");
    
    if (!validarDados()) return;
    setEtapa('contato');
  };

  const handleCadastrar = async (e: React.FormEvent) => {
    e.preventDefault();
    setErro("");
    
    if (!validarContato()) return;
    
    setCarregando(true);
    try {
      const contato: ContatoEmergencia = {
        nome: contatoNome,
        email: contatoEmail,
        whatsapp: contatoWhatsapp,
      };
      
      await cadastrarUsuario({ nome, email, telefone, senha }, contato);
      setSucesso("Cadastro realizado com sucesso!");
      setTimeout(() => {
        // Reset e volta para login (aqui você pode navegar)
        window.location.reload();
      }, 2000);
    } catch (err) {
      setErro("Erro ao cadastrar. Tente novamente.");
    } finally {
      setCarregando(false);
    }
  };

  const handleVoltar = () => {
    setEtapa('dados');
    setErro("");
  };

  if (etapa === 'contato') {
    return (
      <div style={styles.container}>
        <div style={styles.card}>
          <div style={styles.header}>
            <div style={{ fontSize: '2.5rem', marginBottom: '0.5rem' }}>🛡️</div>
            <div style={styles.title}>Contato de Emergência</div>
            <div style={styles.subtitle}>Quem será avisado se você não responder?</div>
          </div>

          <form onSubmit={handleCadastrar}>
            <div style={styles.sectionTitle}>Dados do Contato</div>
            
            <div style={styles.inputGroup}>
              <label style={styles.label}>Nome completo</label>
              <input
                type="text"
                placeholder="Nome do contato"
                value={contatoNome}
                onChange={e => setContatoNome(e.target.value)}
                required
                style={styles.input}
              />
            </div>

            <div style={styles.inputGroup}>
              <label style={styles.label}>E-mail</label>
              <input
                type="email"
                placeholder="contato@email.com"
                value={contatoEmail}
                onChange={e => setContatoEmail(e.target.value)}
                required
                style={styles.input}
              />
            </div>

            <div style={styles.inputGroup}>
              <label style={styles.label}>WhatsApp</label>
              <input
                type="tel"
                placeholder="(11) 99999-9999"
                value={contatoWhatsapp}
                onChange={e => setContatoWhatsapp(e.target.value)}
                required
                style={styles.input}
              />
            </div>

            {erro && <div style={styles.error}>⚠️ {erro}</div>}
            {sucesso && <div style={styles.success}>✅ {sucesso}</div>}

            <button 
              type="submit" 
              disabled={carregando}
              style={{ ...styles.button, opacity: carregando ? 0.6 : 1 }}
            >
              {carregando ? "Cadastrando..." : "Finalizar Cadastro"}
            </button>

            <button 
              type="button" 
              onClick={handleVoltar}
              style={{ ...styles.button, background: '#6b7280', marginTop: '0.75rem' }}
            >
              Voltar
            </button>
          </form>
        </div>
      </div>
    );
  }

  return (
    <div style={styles.container}>
      <div style={styles.card}>
        <div style={styles.header}>
          <div style={{ fontSize: '2.5rem', marginBottom: '0.5rem' }}>🛡️</div>
          <div style={styles.title}>Criar Conta</div>
          <div style={styles.subtitle}>Sistema de Segurança Pessoal</div>
        </div>

        <form onSubmit={handleContinuar}>
          <div style={styles.sectionTitle}>Seus Dados</div>
          
          <div style={styles.inputGroup}>
            <label style={styles.label}>Nome completo</label>
            <input
              type="text"
              placeholder="Seu nome"
              value={nome}
              onChange={e => setNome(e.target.value)}
              required
              style={styles.input}
            />
          </div>

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
            <label style={styles.label}>Telefone (opcional)</label>
            <input
              type="tel"
              placeholder="(11) 99999-9999"
              value={telefone}
              onChange={e => setTelefone(e.target.value)}
              style={styles.input}
            />
          </div>

          <div style={styles.row}>
            <div style={styles.inputGroup}>
              <label style={styles.label}>Senha</label>
              <input
                type="password"
                placeholder="Mínimo 6 caracteres"
                value={senha}
                onChange={e => setSenha(e.target.value)}
                required
                style={styles.input}
              />
            </div>
            <div style={styles.inputGroup}>
              <label style={styles.label}>Confirmar</label>
              <input
                type="password"
                placeholder="Repita a senha"
                value={confirmarSenha}
                onChange={e => setConfirmarSenha(e.target.value)}
                required
                style={styles.input}
              />
            </div>
          </div>

          {erro && <div style={styles.error}>⚠️ {erro}</div>}

          <button 
            type="submit" 
            style={styles.button}
          >
            Continuar
          </button>

          <div style={styles.footer}>
            Já tem conta? <span style={styles.link}>Entrar</span>
          </div>
        </form>
      </div>
    </div>
  );
};