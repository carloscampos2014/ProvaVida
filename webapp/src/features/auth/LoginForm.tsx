import React, { useState } from "react";
import { loginUsuario } from "./authService";
import { Button } from "../../components/Button";

export const LoginForm: React.FC = () => {
  const [email, setEmail] = useState("");
  const [senha, setSenha] = useState("");
  const [erro, setErro] = useState("");
  const [carregando, setCarregando] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setCarregando(true);
    setErro("");
    try {
      const usuario = await loginUsuario({ email, senha });
      alert(`Bem-vindo, ${usuario.nome}!`);
    } catch (err) {
      setErro("Login inválido. Verifique suas credenciais.");
    } finally {
      setCarregando(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} style={{ maxWidth: 320, margin: "2rem auto", display: "flex", flexDirection: "column", gap: 12 }}>
      <h2>Login</h2>
      <input
        type="email"
        placeholder="E-mail"
        value={email}
        onChange={e => setEmail(e.target.value)}
        required
      />
      <input
        type="password"
        placeholder="Senha"
        value={senha}
        onChange={e => setSenha(e.target.value)}
        required
      />
      <Button type="submit" disabled={carregando}>{carregando ? "Entrando..." : "Entrar"}</Button>
      {erro && <span style={{ color: "red" }}>{erro}</span>}
    </form>
  );
};
