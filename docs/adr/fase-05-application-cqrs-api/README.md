# Fase 05 - Aplicação (CQRS), API e Erros RFC 7807

## Escopo

Casos de uso da Application com MediatR (comandos e consultas), auditoria desacoplada por notificação, cálculo de fatias percentuais, endpoints HTTP sob `/api/v1/exchange/` e tratamento global de erros em Problem Details.

## O que foi implementado

- **Comando:** `ConvertCurrencyCommand` e `ConvertCurrencyCommandHandler`. Publica `ConversionExecutedNotification` após calcular a conversão.
- **Evento de auditoria:** `ConversionAuditEventHandler` persiste o registro via `IConversionAuditRepository`.
- **Consultas:** `ValuatePortfolioQuery`, `GetHistoricalRatesQuery`, `GetLatestRatesQuery`, `GetCurrenciesQuery`, `GetAuditLogsQuery`, cada uma com seu handler.
- **Cálculo de fatias:** `SharePercentageCalculator` (Domain), com o método dos maiores restos.
- **Erros:** `GlobalExceptionHandler` (`IExceptionHandler`) produzindo `ProblemDetails`.
- **Endpoints:** seis rotas em `Api/Endpoints/ExchangeEndpoints.cs`.
- **Serialização:** `camelCase` e enums como string, via `ConfigureHttpJsonOptions`.
- **Documentação:** OpenAPI nativo e Scalar (`/scalar/v1`).

## Rotas

| Método | Rota | Caso de uso |
|---|---|---|
| GET | `/api/v1/exchange/convert` | `ConvertCurrencyCommand` |
| POST | `/api/v1/exchange/portfolio/valuate` | `ValuatePortfolioQuery` |
| GET | `/api/v1/exchange/history` | `GetHistoricalRatesQuery` |
| GET | `/api/v1/exchange/latest` | `GetLatestRatesQuery` |
| GET | `/api/v1/exchange/currencies` | `GetCurrenciesQuery` |
| GET | `/api/v1/exchange/audit` | `GetAuditLogsQuery` |

## Decisões

| ADR | Título | Status |
|---|---|---|
| [ADR-016](ADR-016-mediatr-12-e-auditoria-por-notificacao.md) | MediatR 12 e auditoria desacoplada por notificação | Aceito |
| [ADR-017](ADR-017-fatias-percentuais-pelo-metodo-dos-maiores-restos.md) | Fatias percentuais pelo método dos maiores restos | Aceito |
| [ADR-018](ADR-018-erros-rfc-7807-com-iexceptionhandler.md) | Erros em RFC 7807 com `IExceptionHandler` | Aceito |
| [ADR-019](ADR-019-endpoints-minimal-api-e-scalar-em-development.md) | Endpoints em Minimal API e Scalar restrito a Development | Aceito |

## Bugs corrigidos durante a validação

- `ISpreadStrategyResolver` não estava registrado no contêiner; a API falhava ao subir. Corrigido em `AddApplication`, com teste de `ValidateOnBuild`.
- Corpo JSON malformado virava 500; passou a retornar 400 (`BadHttpRequestException`).
- Carteira sem `assets` no corpo causava `NullReferenceException`; passou a retornar 422.
