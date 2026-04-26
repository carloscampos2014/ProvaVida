describe('E2E - Login', () => {
  it('Deve exibir erro ao tentar logar com credenciais inválidas', () => {
    cy.visit('http://localhost:5173');
    cy.get('input[placeholder="E-mail"]').type('naoexiste@teste.com');
    cy.get('input[placeholder="Senha"]').type('senhaerrada');
    cy.get('button[type="submit"]').click();
    cy.contains('Login inválido').should('be.visible');
  });

  // Para testar login real, é necessário backend rodando e usuário válido
  // it('Deve logar com sucesso com credenciais válidas', () => {
  //   cy.visit('http://localhost:5173');
  //   cy.get('input[placeholder="E-mail"]').type('usuario@valido.com');
  //   cy.get('input[placeholder="Senha"]').type('senhaValida123!');
  //   cy.get('button[type="submit"]').click();
  //   cy.on('window:alert', (txt) => {
  //     expect(txt).to.contains('Bem-vindo');
  //   });
  // });
});
