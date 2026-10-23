# Fase 02 - Domínio: Value Objects e Spread

## Escopo

Modelagem da regra de negócio pura do câmbio: moedas e valores monetários imutáveis, cotação, estratégias de spread por perfil de cliente e o resultado consolidado de uma conversão. Todo o código está em `SmartExchange.Domain`, sem dependências externas.

## O que foi implementado

- **Value objects:** `Currency` (código ISO 4217 de 3 letras, normalizado para maiúsculas) e `Money` (`decimal Amount` + `Currency`), ambos imutáveis.
- **Enum:** `ClientTier` (`Standard`, `Premium`, `Vip`).
- **Exceções:** `DomainException` como única exceção de regra de negócio do domínio.
- **Cotação:** `ExchangeRate` (moeda base, moeda destino, taxa positiva, timestamp UTC).
- **Spread:** `ISpreadStrategy` com as implementações `StandardSpreadStrategy` (3,5%), `PremiumSpreadStrategy` (1,5%) e `VipSpreadStrategy` (0,5%), e `SpreadStrategyResolver` para escolher a estratégia pelo `ClientTier`.
- **Resultado:** `ConversionResult.Calculate(...)` consolida origem, destino, taxa base, taxa com spread, percentual, valor do spread, perfil e timestamp.
- **Arredondamento centralizado:** `FinancialRounding`.

## Decisões

| ADR | Título | Status |
|---|---|---|
| [ADR-005](ADR-005-currency-como-record-class.md) | `Currency` como `sealed record`, e não `readonly record struct` | Aceito |
| [ADR-006](ADR-006-arredondamento-financeiro.md) | Arredondamento explícito com `ToEven` (6 casas em taxas, 2 em quantias) | Aceito |
| [ADR-007](ADR-007-money-rejeita-valores-negativos.md) | `Money` rejeita valores negativos e operações entre moedas diferentes | Aceito |
| [ADR-008](ADR-008-strategy-pattern-para-spread.md) | Strategy Pattern para spread, com resolução por `ClientTier` | Aceito |

## Qualidade

- Cobertura de linha e de ramificação em 100% nas classes do domínio, conforme a constitution da fase.
- Os cálculos de spread e de `Money` são cobertos com valores de borda: divisão por zero, valores negativos e empate de arredondamento.
