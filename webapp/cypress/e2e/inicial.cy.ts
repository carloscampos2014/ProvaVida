describe('E2E - ProvaVida WebApp', () => {
  it('Deve exibir o título inicial', () => {
    cy.visit('http://localhost:5173');
    cy.contains('ProvaVida WebApp - Sprint 5');
  });
});
