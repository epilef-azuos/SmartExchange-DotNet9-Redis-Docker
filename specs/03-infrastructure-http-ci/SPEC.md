# SPEC: Fase 03 - Cliente Frankfurter API, Resiliência Polly e Pipeline CI

## Objetivo
Implementar o cliente HTTP tipado para consumo da Frankfurter API pública, configurar o pipeline nativo de resiliência do .NET 9, validar o comportamento com testes unitários mockando o handler HTTP e criar a automação de CI via GitHub Actions.

## Requisitos Técnicos & Contratos

### 1. Contrato da Aplicação (`SmartExchange.Application`)
- **`IExchangeRateProvider`**:
  - `Task<ExchangeRate> GetLatestRateAsync(Currency baseCurrency, Currency targetCurrency, CancellationToken ct = default);`
  - `Task<IReadOnlyDictionary<string, decimal>> GetLatestRatesAsync(Currency baseCurrency, CancellationToken ct = default);`
  - `Task<IReadOnlyDictionary<string, string>> GetCurrenciesAsync(CancellationToken ct = default);`
  - `Task<IReadOnlyDictionary<DateOnly, Dictionary<string, decimal>>> GetHistoricalRatesAsync(Currency baseCurrency, Currency targetCurrency, DateOnly startDate, DateOnly endDate, CancellationToken ct = default);`

### 2. Infraestrutura HTTP (`SmartExchange.Infrastructure`)
- Pacotes necessários:
  - `Microsoft.Extensions.Http.Resilience` (v9.x)
  - `Microsoft.Extensions.Options.ConfigurationExtensions`
- **`FrankfurterApiClient`**:
  - Consumo da base `https://api.frankfurter.app`.
  - Mapeamento de DTOs JSON internos para os modelos de domínio (`ExchangeRate`, etc.).
  - Tratamento de status code HTTP: conversão de erros 4xx/5xx para exceções descritivas de infraestrutura (`ExchangeIntegrationException`).
- **Pipeline de Resiliência Nativo**:
  - **Retry:** 3 tentativas com backoff exponencial com jitter.
  - **Circuit Breaker:** Abertura após taxa de amostragem de falhas (ex: 50% de falhas em uma janela de 10s), quebrando o circuito temporariamente.
  - **Timeout:** Timeout total por tentativa de requisição (ex: 5 segundos).

### 3. Testes Unitários de Integração (`SmartExchange.Tests.Unit`)
- `Mock<HttpMessageHandler>`:
  - Cenário de sucesso de desserialização (200 OK).
  - Cenário de falha 500/503 e validação do disparo de retries.
  - Cenário de retorno malformado ou payload inválido.

### 4. GitHub Actions CI (`.github/workflows/ci.yml`)
- Trigger em `push` e `pull_request` para `main` e `feature/**`.
- Etapas:
  1. Checkout do repositório.
  2. Setup .NET 9 SDK (usando o `global.json`).
  3. `dotnet restore`.
  4. `dotnet build --no-restore --configuration Release`.
  5. `dotnet test --no-build --configuration Release --verbosity normal`.