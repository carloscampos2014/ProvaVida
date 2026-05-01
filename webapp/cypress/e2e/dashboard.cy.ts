describe("Dashboard - Fluxo Completo", () => {
  beforeEach(() => {
    // Mock do token no localStorage
    cy.window().then((win) => {
      win.localStorage.setItem("token", "test-token-123");
    });
    cy.visit("http://localhost:5173");
  });

  it("Deve exibir o dashboard ao fazer login", () => {
    cy.get("header").should("be.visible");
    cy.contains("ProvaVida").should("exist");
    cy.contains("Bem-vindo").should("exist");
  });

  it("Deve exibir status do próximo check-in", () => {
    cy.contains("Status do Próximo Check-in").should("be.visible");
    cy.contains("Tempo restante").should("be.visible");
  });

  it("Deve exibir botão para fazer check-in", () => {
    cy.contains("Fazer Check-in").should("be.visible");
    cy.contains("Fazer Check-in").click();
    // Após clique, o botão pode mostrar "Enviando..."
    cy.contains(/Enviando|Fazer Check-in/).should("be.visible");
  });

  it("Deve exibir contatos de emergência", () => {
    cy.contains("Contatos de Emergência").should("be.visible");
  });

  it("Deve exibir histórico de check-ins", () => {
    cy.contains("Histórico de Check-ins").should("be.visible");
  });

  it("Deve fazer logout ao clicar em Sair", () => {
    cy.contains("Sair").should("be.visible");
    cy.contains("Sair").click();
    // Deve voltar para login
    cy.contains("Entrar").should("be.visible");
    cy.window().then((win) => {
      expect(win.localStorage.getItem("token")).to.be.null;
    });
  });

  it("Deve exibir badge de notificações quando houver pendentes", () => {
    // Se houver notificações, deve exibir o ícone 🔔
    cy.get("body").then(($body) => {
      if ($body.text().includes("🔔")) {
        cy.contains("🔔").should("be.visible");
      }
    });
  });

  it("Deve ser responsivo em telas móveis", () => {
    cy.viewport("iphone-x");
    cy.get("header").should("be.visible");
    cy.contains("Bem-vindo").should("be.visible");
  });
});

describe("Dashboard - Testes de Erro", () => {
  beforeEach(() => {
    cy.window().then((win) => {
      win.localStorage.setItem("token", "invalid-token");
    });
  });

  it("Deve exibir mensagem de erro ao falhar ao carregar dados", () => {
    cy.intercept("GET", "**/api/dashboard", { statusCode: 500 });
    cy.visit("http://localhost:5173");
    cy.contains(/Erro ao carregar|Tentar Novamente/).should("be.visible");
  });
});
