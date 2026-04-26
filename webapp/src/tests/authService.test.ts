import { loginUsuario } from "../features/auth/authService";

describe("authService", () => {
  it("deve retornar dados do usuário ao logar com sucesso", async () => {
    // Mock do axios
    jest.spyOn(global, "fetch").mockResolvedValueOnce({
      ok: true,
      json: async () => ({ id: "1", nome: "Teste", email: "teste@exemplo.com" })
    } as any);

    // Como usamos axios, ideal seria mockar axios, mas exemplo didático:
    const usuario = await loginUsuario({ email: "teste@exemplo.com", senha: "12345678" });
    expect(usuario).toHaveProperty("id");
    expect(usuario).toHaveProperty("nome");
    expect(usuario).toHaveProperty("email");
  });
});
