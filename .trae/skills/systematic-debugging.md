# Skill: Systematic Debugging

Diagnóstico estruturado para corrigir bugs — da hipótese à correção verificada.

## Quando usar

Ativar quando: bug reportado, comportamento inesperado, erro intermitente, performance degradada.

## Processo

1. **Definir precisamente** — o que acontece vs o que deveria acontecer, quando e desde quando
2. **Reproduzir isoladamente** — criar teste que reproduza o bug antes de corrigir
3. **Formular hipóteses** — ordenadas por probabilidade, baseadas em mudanças recentes e logs
4. **Testar uma por vez** — nunca aplicar múltiplas correções simultâneas
5. **Correção mínima** — menor mudança que resolve a causa raiz (sem workarounds)
6. **Twin Check** — buscar o mesmo padrão em todo o projeto
7. **Verificar** — build + testes passando; mostrar evidência

## Hard bound

Após 3 hipóteses refutadas sem progresso: parar, reportar o que foi tentado e devolver ao usuário.

## Nunca

- Corrigir sem conseguir reproduzir
- Usar `catch {}` para suprimir o erro
- `if (valor == null) return` sem entender por que é null
- Considerar "concluído" sem evidência observada
