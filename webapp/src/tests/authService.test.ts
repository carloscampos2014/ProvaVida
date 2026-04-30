import { loginUsuario } from "../features/auth/authService";
import api from "../services/api";

jest.mock("../services/api");

describe("authService", () => {
  it("deve retornar dados do usuário ao logar com sucesso", async () => {
    const mockResponse = { id: "1", nome: "Teste", email: "teste@exemplo.com" };
    (api.post as jest.Mock).mockResolvedValueOnce({ data: mockResponse });

    const usuario = await loginUsuario({ email: "teste@exemplo.com", senha: "12345678" });
    expect(usuario).toHaveProperty("id");
    expect(usuario).toHaveProperty("nome");
    expect(usuario).toHaveProperty("email");
    expect(api.post).toHaveBeenCalledWith("/auth/login", { email: "teste@exemplo.com", senha: "12345678" });
  });
});
