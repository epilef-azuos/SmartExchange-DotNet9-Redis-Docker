# PLAN: Fase 05

1. **Branch e Pacotes da Camada Application e API**
   - Criar a branch `feature/05-application-cqrs-api`.
   - Adicionar o pacote `MediatR` em `SmartExchange.Application`.
   - Configurar registro de injeção de dependência em `SmartExchange.Application/DependencyInjection.cs`.
2. **Implementação dos Casos de Uso (Application)**
   - Criar DTOs de entrada e saída em `SmartExchange.Application/Exchange/Dtos/`.
   - Implementar `ConvertCurrencyCommand`, `ConvertCurrencyCommandHandler` e `ConversionExecutedNotification`.
   - Implementar `ConversionAuditHandler` para persistência assíncrona do log de auditoria.
   - Implementar `ValuatePortfolioQuery` e seu Handler com cálculos de proporção.
   - Implementar `GetHistoricalRatesQuery`, `GetLatestRatesQuery` e `GetCurrenciesQuery`.
   - Implementar `GetAuditLogsQuery` e Handler de leitura direta do repositório.
3. **Exposição dos Endpoints (Api)**
   - Criar `ExchangeController.cs` (ou Minimal APIs organizadas em Endpoints) mapeando as 6 rotas sob `/api/v1/exchange/`.
   - Configurar o tratamento global de erros (`GlobalExceptionHandler.cs` implementando `IExceptionHandler`).
   - Configurar serialização JSON para converter enums como strings (`JsonStringEnumConverter`) e formatar decimais com precisão.
4. **Testes Unitários da Camada Application**
   - Criar testes unitários para `ConvertCurrencyCommandHandler` em `tests/SmartExchange.Tests.Unit/Application/ConvertCurrencyCommandHandlerTests.cs`.
   - Criar testes unitários para `ValuatePortfolioQueryHandler` validando a precisão percentual das fatias da carteira.
5. **Validação, Compilação e Atualização da Tarefa**
   - Executar `dotnet build` e `dotnet test`.
   - Garantir 100% de sucesso e atualizar o `TASK.md`.