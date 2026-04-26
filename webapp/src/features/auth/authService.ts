import api from "../services/api";

export interface UsuarioLoginDto {
  email: string;
  senha: string;
}

export interface UsuarioResumoDto {
  id: string;
  nome: string;
  email: string;
}

export async function loginUsuario(dados: UsuarioLoginDto): Promise<UsuarioResumoDto> {
  const resposta = await api.post("/auth/login", dados);
  return resposta.data;
}
