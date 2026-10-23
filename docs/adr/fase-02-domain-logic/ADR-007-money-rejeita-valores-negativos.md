# ADR-007: `Money` rejeita valores negativos e operações entre moedas diferentes

- **Status:** Aceito
- **Fase:** 02 - Domínio

## Contexto

A spec define que o montante de uma transação de câmbio não pode ser negativo. Operações aritméticas podem produzir negativos (por exemplo, uma subtração maior que o saldo) ou misturar moedas, e ambos os casos seriam erros silenciosos se não fossem barrados.

## Decisão

- O construtor de `Money` lança `DomainException` quando `Amount < 0` ou quando a moeda é nula.
- `Add` e `Subtract` exigem a mesma moeda; caso contrário, lançam `DomainException`.
- `Multiply` com fator negativo e `Subtract` que resulta em negativo lançam exceção, pelo próprio construtor.
- `Divide` por zero lança `DomainException` explícita.
- Os operadores `+`, `-`, `*` e `/` apenas delegam aos métodos nomeados.
- Operações retornam novas instâncias; a original não é alterada.

## Consequências

- Não existe `Money` negativo em memória, o que simplifica quem consome o objeto.
- A regra "montante positivo" da entrada HTTP é validada na Application (`ValidationException`, 422), e a do domínio é a garantia final (`DomainException`, 400). Ver ADR-018.
