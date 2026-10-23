# TASK: Casos de Uso MediatR (CQRS), Endpoints da API e Auditoria

Status Geral: CONCLUÍDA (código, testes e validação de endpoints; commit pendente)
Fase: 05 - Application CQRS & API
Branch: feature/05-application-cqrs-api

---

## 1. Controle de Versão e Dependências
- [x] Criar e chavear para a branch `feature/05-application-cqrs-api`.
- [x] Adicionar o pacote `MediatR` em `src/SmartExchange.Application`.
- [x] Criar `DependencyInjection.cs` em `src/SmartExchange.Application` registrando o MediatR.

## 2. DTOs e Notificações (Application)
- [x] Criar DTOs de resposta (`ConversionResponse.cs`, `PortfolioValuationResponse.cs`, `HistoricalRatesResponse.cs`, `AuditLogDto.cs`) em `src/SmartExchange.Application/Exchange/Dtos/`.
- [x] Criar evento `ConversionExecutedNotification.cs` em `src/SmartExchange.Application/Exchange/Events/`.
- [x] Criar manipulador de evento `ConversionAuditEventHandler.cs` em `src/SmartExchange.Application/Exchange/EventHandlers/` gravando no `IConversionAuditRepository`.

## 3. Casos de Uso CQRS (Application)
- [x] Implementar `ConvertCurrencyCommand.cs` e `ConvertCurrencyCommandHandler.cs` (orquestrando cotação, cálculo de spread e notificação).
- [x] Implementar `ValuatePortfolioQuery.cs` e `ValuatePortfolioQueryHandler.cs` (cálculo de valor total e fatias percentuais `SharePercentage`).
- [x] Implementar `GetHistoricalRatesQuery.cs` e `GetHistoricalRatesQueryHandler.cs` (resolvendo janelas de período 7d/30d/90d).
- [x] Implementar `GetLatestRatesQuery.cs` e `GetLatestRatesQueryHandler.cs`.
- [x] Implementar `GetCurrenciesQuery.cs` e `GetCurrenciesQueryHandler.cs`.
- [x] Implementar `GetAuditLogsQuery.cs` e `GetAuditLogsQueryHandler.cs`.

## 4. Camada de Apresentação, Endpoints e Scalar (Api)
- [x] Adicionar os pacotes `Microsoft.AspNetCore.OpenApi` e `Scalar.AspNetCore` no projeto `src/SmartExchange.Api`.
- [x] Configurar no `Program.cs` o pipeline do OpenAPI e Scalar (`app.MapOpenApi()` e `app.MapScalarApiReference()`).
- [x] Implementar `GlobalExceptionHandler.cs` com suporte a RFC 7807 (ProblemDetails) em `src/SmartExchange.Api/Middleware/`.
- [x] Configurar Serialização JSON em `Program.cs` (`JsonStringEnumConverter`, camelCase).
- [x] Registrar os 6 endpoints da API (`ExchangeController.cs` ou Endpoints de Extensão) cobrindo:
  - `GET /api/v1/exchange/convert`
  - `POST /api/v1/exchange/portfolio/valuate`
  - `GET /api/v1/exchange/history`
  - `GET /api/v1/exchange/latest`
  - `GET /api/v1/exchange/currencies`
  - `GET /api/v1/exchange/audit`

## 5. Testes Unitários de Casos de Uso
- [x] Implementar `ConvertCurrencyCommandHandlerTests.cs` cobrindo o fluxo de conversão com spread e publicação da notificação.
- [x] Implementar `ValuatePortfolioQueryHandlerTests.cs` validando o somatório total e a proporcionalidade de 100% da carteira.

## 6. Validação e Fechamento
- [x] Executar `dotnet build` sem erros ou warnings.
- [x] Executar `dotnet test` garantindo que todos os testes passem.
- [x] Marcar todos os checkboxes deste `TASK.md` como `[x]`.