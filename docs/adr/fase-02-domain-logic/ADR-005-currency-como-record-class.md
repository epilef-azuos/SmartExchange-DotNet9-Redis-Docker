# ADR-005: `Currency` como `sealed record`, e não `readonly record struct`

- **Status:** Aceito
- **Fase:** 02 - Domínio

## Contexto

A constitution da Fase 02 prefere `readonly record struct` para os value objects. O problema é que uma struct tem um construtor padrão (`default(Currency)`) que não passa pelo construtor validado. Assim, `default(Currency)` produziria um objeto com `Code` nulo, o que contorna a regra de "auto-validação".

## Decisão

`Currency` é um `sealed record` (classe), com validação no construtor, `Code` apenas de leitura e igualdade por valor.

## Consequências

- Não existe instância inválida de `Currency`, nem por `default`.
- O custo é uma alocação de heap por moeda. Em volume de conversões, isso é desprezível frente à chamada de rede.
- Desvio consciente da preferência textual da constitution; a intenção (imutabilidade e validação) foi preservada.
