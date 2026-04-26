import { render, screen } from "@testing-library/react";
import { Button } from "../components/Button";

describe("Button", () => {
  it("renderiza o texto corretamente", () => {
    render(<Button>Entrar</Button>);
    expect(screen.getByText("Entrar")).toBeInTheDocument();
  });
});
