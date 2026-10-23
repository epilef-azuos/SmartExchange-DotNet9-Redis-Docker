# Fase 03 - Infraestrutura HTTP, Resiliência e CI

## Escopo

Cliente tipado para a Frankfurter API, com resiliência nativa do .NET 9 e tratamento de erros de integração, mais a pipeline de integração contínua no GitHub Actions.

## O que foi implementado

- **Contrato na Application:** `IExchangeRateProvider` (cotação pontual, cotações por base, moedas e histórico) e `ExchangeIntegrationException`.
- **Cliente:** `FrankfurterApiClient` em `Infrastructure/Http/Clients`, com DTOs de deserialização separados do domínio.
- **Resiliência:** pipeline `AddStandardResilienceHandler` customizado, com retry exponencial com jitter, circuit breaker e timeouts.
- **Configuração:** `FrankfurterOptions` (seção `Frankfurter:BaseUrl`), com padrão `https://api.frankfurter.app`.
- **Testes:** `FrankfurterApiClientTests` com `Mock<HttpMessageHandler>`, sem nenhuma chamada de rede real.
- **CI:** `.github/workflows/ci.yml`.

## Decisões

| ADR | Título | Status |
|---|---|---|
| [ADR-009](ADR-009-inversao-de-dependencia-do-provedor.md) | Contrato `IExchangeRateProvider` na Application | Aceito |
| [ADR-010](ADR-010-resiliencia-com-microsoft-extensions-http-resilience.md) | Resiliência com `Microsoft.Extensions.Http.Resilience` | Aceito |
| [ADR-011](ADR-011-host-legado-da-frankfurter-api.md) | Host legado `api.frankfurter.app` | Aceito, com risco de depreciação |
| [ADR-012](ADR-012-pipeline-github-actions.md) | Pipeline de CI no GitHub Actions | Aceito |

## Observações

- A Fase 03 foi consolidada em `main` pela PR #2.
- Os testes de integração desta fase com a Frankfurter real não existem; a substituição por fake nos testes de integração da Fase 06 está em ADR-022.
