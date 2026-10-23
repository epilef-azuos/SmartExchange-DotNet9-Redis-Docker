# TASK: Infraestrutura HTTP, Resiliência Nativa e Pipeline CI

Status Geral: EM PROGRESSO
Fase: 03 - Infrastructure HTTP & CI
Branch: feature/03-infrastructure-http-ci

---

## 1. Controle de Versão e Dependências
- [x] Criar e chavear para a branch `feature/03-infrastructure-http-ci`.
- [x] Adicionar o pacote `Microsoft.Extensions.Http.Resilience` ao projeto `src/SmartExchange.Infrastructure`.

## 2. Abstrações e Contratos (Application)
- [x] Criar a interface `IExchangeRateProvider.cs` em `src/SmartExchange.Application/Common/Interfaces/`.
- [x] Criar a classe de exceção `ExchangeIntegrationException.cs` em `src/SmartExchange.Application/Common/Exceptions/`.

## 3. Implementação do Cliente Frankfurter (Infrastructure)
- [x] Criar os records de resposta JSON (`FrankfurterLatestResponse.cs`, etc.) em `src/SmartExchange.Infrastructure/Http/Dtos/`.
- [x] Implementar a classe `FrankfurterApiClient.cs` implementando `IExchangeRateProvider`.
- [x] Criar classe estática de extensão `DependencyInjection.cs` em `src/SmartExchange.Infrastructure/` configurando o `AddHttpClient<IExchangeRateProvider, FrankfurterApiClient>` com `AddStandardResilienceHandler` ou pipeline customizada de Retry + Circuit Breaker.

## 4. Testes Unitários de HTTP e Resiliência
- [x] Criar `MockHttpMessageHandler` ou helper de teste em `tests/SmartExchange.Tests.Unit/Helpers/`.
- [x] Implementar testes unitários em `tests/SmartExchange.Tests.Unit/Infrastructure/Http/FrankfurterApiClientTests.cs` cobrindo:
  - Desserialização de cotações com sucesso (200 OK).
  - Resposta de erro da Frankfurter API gerando `ExchangeIntegrationException`.
  - Simulação de transient failure (HTTP 500/503).

## 5. Esteira de Integração Contínua (GitHub Actions)
- [x] Criar diretório `.github/workflows/`.
- [x] Criar arquivo `.github/workflows/ci.yml` configurado para .NET 9 executando restore, build e test para eventos de `push` e `pull_request`.

## 6. Validação e Finalização
- [x] Executar `dotnet build` localmente garantindo 0 warnings e 0 errors.
- [x] Executar `dotnet test` localmente garantindo 100% de sucesso.
- [x] Garantir que todas as caixas de seleção deste arquivo `TASK.md` estejam marcadas com `[x]`.