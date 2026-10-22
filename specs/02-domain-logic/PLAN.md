# PLAN: Fase 02

1. **Branch e Estrutura de Pastas no Domínio**
   - Criar a branch `feature/02-domain-logic`.
   - Organizar a estrutura em `src/SmartExchange.Domain/`:
     - `Common/` (exceções de domínio e classes base)
     - `ValueObjects/` (`Currency.cs`, `Money.cs`)
     - `Enums/` (`ClientTier.cs`)
     - `Strategies/` (`ISpreadStrategy.cs`, implementações de spread)
     - `Models/` (`ConversionResult.cs`, `ExchangeRate.cs`)
2. **Implementação dos Value Objects e Exceptions**
   - Implementar `DomainException.cs`.
   - Implementar `Currency` com validação de regex/tamanho ISO 4217.
   - Implementar `Money` com operadores aritméticos básicos seguros.
3. **Implementação das Estratégias de Spread**
   - Definir `ISpreadStrategy`.
   - Implementar `StandardSpreadStrategy`, `PremiumSpreadStrategy` e `VipSpreadStrategy`.
   - Implementar `SpreadStrategyResolver`.
4. **Implementação da Suíte de Testes Unitários**
   - Criar testes unitários em `tests/SmartExchange.Tests.Unit/Domain/`:
     - `CurrencyTests.cs`
     - `MoneyTests.cs`
     - `SpreadStrategyTests.cs`
5. **Validação Final de Compilação e Execução de Testes**
   - Executar suíte completa via `dotnet test`.