# TASK: Core Domain, Value Objects e Strategy Pattern

Status Geral: EM PROGRESSO
Fase: 02 - Domain Logic
Branch: feature/02-domain-logic

---

## 1. Controle de Versão e Estrutura do Domínio
- [x] Criar e chavear para a branch `feature/02-domain-logic` no Git.
- [x] Criar a estrutura de diretórios em `src/SmartExchange.Domain`: `Common/`, `ValueObjects/`, `Enums/`, `Strategies/`, `Models/`.
- [x] Criar `DomainException.cs` em `Common/`.

## 2. Value Objects e Enums
- [x] Implementar `ClientTier.cs` (`Standard`, `Premium`, `Vip`) em `Enums/`.
- [x] Implementar `Currency.cs` em `ValueObjects/` com validação estrita de 3 letras alfabéticas maiúsculas.
- [x] Implementar `Money.cs` em `ValueObjects/` garantindo imutabilidade e operações matemáticas seguras com `decimal`.
- [x] Implementar `ExchangeRate.cs` em `Models/` (moeda base, moeda destino, taxa, timestamp UTC).

## 3. Strategy Pattern para Spread
- [x] Criar interface `ISpreadStrategy.cs` em `Strategies/`.
- [x] Implementar `StandardSpreadStrategy.cs` (spread de 3.5%).
- [x] Implementar `PremiumSpreadStrategy.cs` (spread de 1.5%).
- [x] Implementar `VipSpreadStrategy.cs` (spread de 0.5%).
- [x] Implementar `ISpreadStrategyResolver.cs` e `SpreadStrategyResolver.cs` em `Strategies/`.
- [x] Implementar `ConversionResult.cs` em `Models/` encapsulando os dados completos da conversão calculada.

## 4. Testes Unitários (TDD / Validação)
- [x] Criar testes unitários para `Currency` validando códigos válidos e exceções para formatos incorretos.
- [x] Criar testes unitários para `Money` validando operações, valores negativos e imutabilidade.
- [x] Criar testes unitários para cada `ISpreadStrategy` verificando a precisão decimal dos percentuais e montantes.
- [x] Criar testes unitários para `SpreadStrategyResolver` garantindo a resolução correta para cada `ClientTier`.

## 5. Validação de Build e Rastreabilidade
- [x] Executar `dotnet build` e garantir compilação limpa (0 warnings, 0 errors).
- [x] Executar `dotnet test` e garantir que todos os testes passem com 100% de sucesso.
- [x] Confirmar que todas as caixas de seleção deste arquivo `TASK.md` estão marcadas com `[x]`.