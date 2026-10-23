# ADR-006: Arredondamento explícito com `ToEven` (6 casas em taxas, 2 em quantias)

- **Status:** Aceito
- **Fase:** 02 - Domínio

## Contexto

Valores monetários e taxas são calculados com `decimal`, mas o resultado precisa de uma escala definida. Sem regra explícita, cada cálculo escolhe a própria escala e o arredondamento de empate (`0,025`) pode divergir entre telas, relatórios e banco.

## Decisão

- Taxas e percentuais: `Math.Round(value, 6, MidpointRounding.ToEven)`.
- Quantias monetárias: `Math.Round(value, 2, MidpointRounding.ToEven)`.
- A regra fica centralizada em `FinancialRounding`; nenhuma camada aplica arredondamento por conta própria.
- `ToEven` (arredondamento bancário) foi escolhido em vez de `AwayFromZero`, para que os empates não tendam sistematicamente para cima.

## Consequências

- Os testes fixam o caso de borda: `5 × 0,005 = 0,025` resulta em `0,02`.
- A mesma escala é usada na persistência (`HasPrecision(18, 6)` e `HasPrecision(18, 2)`, ver ADR-014), o que mantém o valor gravado igual ao calculado.
- Mudar a regra de arredondamento afeta todos os cálculos de uma vez, e isso deve ser tratado como mudança de contrato.
