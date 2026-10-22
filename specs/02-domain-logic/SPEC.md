# SPEC: Fase 02 - Core Domain Logic & Spread Strategies

## Objetivo
Implementar as entidades fundamentais, Value Objects monetários, o Strategy Pattern para cálculo de spread cambial baseado no perfil do cliente e os testes unitários da camada de domínio.

## Requisitos de Negócio & Modelagem

### 1. Value Objects & Enums
- **`Currency`**: Código ISO 4217 de 3 letras (ex: "USD", "BRL", "EUR").
  - Regra: Deve ser sanitizado para caixa alta e conter exatamente 3 caracteres alfabéticos.
- **`Money`**: Combinação de `decimal Amount` e `Currency Currency`.
  - Regra: `Amount` não pode ser negativo em transações de câmbio normais.
- **`ClientTier`**: Enum contendo `Standard`, `Premium` e `Vip`.

### 2. Strategy Pattern: Cálculo de Spread
Taxas a aplicar sobre a cotação base ou montante:
- **`StandardSpreadStrategy`**: Spread de 3.5% (`0.035m`).
- **`PremiumSpreadStrategy`**: Spread de 1.5% (`0.015m`).
- **`VipSpreadStrategy`**: Spread de 0.5% (`0.005m`).

### 3. Contratos de Domínio
- **`ISpreadStrategy`**:
  - `ClientTier Tier { get; }`
  - `decimal SpreadPercentage { get; }`
  - `decimal CalculateSpreadAmount(decimal baseAmount);`
  - `decimal ApplySpreadToRate(decimal baseRate);`
- **`ISpreadStrategyResolver`**: Serviço de domínio responsável por obter a estratégia correta a partir do `ClientTier`.
- **`ConversionResult`**: Registro com o resultado consolidado da conversão:
  - Moeda/Valor de Origem, Moeda/Valor de Destino, Taxa Base, Taxa com Spread, Percentual de Spread, Valor do Spread, Perfil do Cliente e Timestamp UTC.