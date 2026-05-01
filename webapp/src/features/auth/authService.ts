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

export interface UsuarioResumoDtoApiResponse {
  dados: UsuarioResumoDto;
  mensagem?: string;
  timestamp: string;
}

export interface ContatoEmergenciaNovoDto {
  nome: string;
  email: string;
  whatsApp: string;
}

export interface CadastroUsuarioDto {
  nome: string;
  email: string;
  telefone?: string;
  senha: string;
  contatoEmergencia: ContatoEmergenciaNovoDto;
}

export async function loginUsuario(dados: UsuarioLoginDto): Promise<UsuarioResumoDtoApiResponse> {
  const resposta = await api.post("/api/v1/Auth/login", dados);
  return resposta.data;
}

export async function cadastrarUsuario(
  usuario: Omit<CadastroUsuarioDto, "contatoEmergencia">,
  contatoEmergencia: ContatoEmergenciaNovoDto
): Promise<UsuarioResumoDtoApiResponse> {
  const dados: CadastroUsuarioDto = {
    ...usuario,
    contatoEmergencia,
  };
  const resposta = await api.post("/api/v1/Auth/registrar", dados);
  return resposta.data;
}
