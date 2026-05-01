import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { LoginForm } from "../features/auth/LoginForm";

jest.mock("../features/auth/authService", () => ({
  loginUsuario: jest.fn(async () => ({ id: "1", nome: "Teste", email: "teste@exemplo.com" }))
}));

describe("LoginForm", () => {
  it("deve exibir mensagem de boas-vindas ao login bem-sucedido", async () => {
    window.alert = jest.fn();
    render(<LoginForm />);
    fireEvent.change(screen.getByPlaceholderText("seu@email.com"), { target: { value: "teste@exemplo.com" } });
    fireEvent.change(screen.getByPlaceholderText("••••••••"), { target: { value: "12345678" } });
    fireEvent.click(screen.getByText("Entrar"));
    await waitFor(() => expect(window.alert).toHaveBeenCalledWith("Bem-vindo, Teste!"));
  });
});
