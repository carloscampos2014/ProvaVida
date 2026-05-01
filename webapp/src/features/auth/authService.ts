import api from "../../services/api";

export interface UsuarioLoginDto {
  email: string;
  senha: string;
}

export interface UsuarioResumoDto {
  id: string;
  nome: string;
  email: string;
}

export interface ContatoEmergenciaDto {
  nome: string;
  email: string;
  whatsapp: string;
}

export interface CadastroUsuarioDto {
  nome: string;
  email: string;
  telefone?: string;
  senha: string;
  contatoEmergencia: ContatoEmergenciaDto;
}

export async function loginUsuario(dados: UsuarioLoginDto): Promise<UsuarioResumoDto> {
  const resposta = await api.post("/auth/login", dados);
  return resposta.data.dados;
}

export async function cadastrarUsuario(
  usuario: Omit<CadastroUsuarioDto, "contatoEmergencia">,
  contatoEmergencia: ContatoEmergenciaDto
): Promise<UsuarioResumoDto> {
  const dados: CadastroUsuarioDto = {
    ...usuario,
    contatoEmergencia,
  };
  // Rota correta: /api/v1/auth/registrar
  const resposta = await api.post("/auth/registrar", dados);
  return resposta.data.dados;
}
