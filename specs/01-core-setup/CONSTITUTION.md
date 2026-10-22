# Constitution: Fase 01 - Setup & Core Domain

1. **Domínio Isolado:** O projeto `Domain` não deve ter nenhuma dependência externa, exceto frameworks de teste nos projetos de teste correspondentes. Nem mesmo injeção de dependência ou pacotes de banco de dados.
2. **Tipagem Financeira:** Todo cálculo envolvendo dinheiro ou taxas deve utilizar estritamente o tipo `decimal`. O uso de `double` ou `float` é terminantemente proibido para evitar erros de ponto flutuante.
3. **Value Objects:** As moedas e os valores devem ser encapsulados em Value Objects imutáveis.
4. **Passos Atômicos:** O setup inicial deve gerar uma solução que compile com sucesso ("0 warnings, 0 errors") antes de escrever a primeira linha de lógica de domínio.