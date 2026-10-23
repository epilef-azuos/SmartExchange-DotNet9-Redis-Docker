# SPEC: Fase 05 - Casos de Uso MediatR, API Gateway BFF e Documentação Interativa com Scalar

## Objetivo
Implementar os Casos de Uso da camada Application utilizando CQRS com MediatR, expor os 6 endpoints RESTful na camada Api, configurar serialização JSON padronizada, tratar exceções globalmente com RFC 7807 (Problem Details), desacoplar o log de auditoria via notificações assíncronas e fornecer documentação de API moderna e interativa utilizando o Scalar.

---

## 1. Documentação Interativa (Scalar no .NET 9)
- **Pacotes na API (`SmartExchange.Api`):**
  - `Microsoft.AspNetCore.OpenApi` (nativo do .NET 9)
  - `Scalar.AspNetCore` (v2.x)
- **Configuração no Pipeline:**
  - Registro de OpenAPI: `builder.Services.AddOpenApi();`
  - Mapeamento Scalar: `app.MapOpenApi();` seguido de `app.MapScalarApiReference(options => { options.WithTitle("SmartExchange Gateway API"); });`
  - Rota padrão acessível no navegador: `/scalar/v1`

---

## 2. Contratos e Regras dos 6 Endpoints da API (`SmartExchange.Api`)

### Endpoint 1: Conversão com Spread Dinâmico
- **Rota:** `GET /api/v1/exchange/convert`
- **Query Params:**
  - `from` (string, ISO-3 obrigatório, ex: `USD`)
  - `to` (string, ISO-3 obrigatório, ex: `BRL`)
  - `amount` (decimal, > 0)
  - `tier` (string opcional: `Standard`, `Premium`, `Vip` - default: `Standard`)
- **Caso de Uso:** `ConvertCurrencyCommand` -> Retorna `ConversionResponse`
- **Fluxo:**
  1. Valida códigos de moeda e montante.
  2. Obtém cotação base via `IExchangeRateProvider`.
  3. Resolve a estratégia via `ISpreadStrategyResolver` usando o `ClientTier`.
  4. Calcula taxa com spread, montante convertido e valor retido de spread.
  5. Publica o evento `ConversionExecutedNotification` via `IMediator.Publish()` para persistência desvinculada.
  6. Retorna payload completo ao cliente com status `200 OK`.

### Endpoint 2: Valoração de Carteira (Portfolio BFF)
- **Rota:** `POST /api/v1/exchange/portfolio/valuate`
- **Payload de Entrada:**
```json
{
  "targetCurrency": "USD",
  "assets": [
    { "currency": "EUR", "amount": 1000.00 },
    { "currency": "BRL", "amount": 5000.00 },
    { "currency": "USD", "amount": 250.00 }
  ]
}
```
- **Caso de Uso:** `ValuatePortfolioQuery` -> Retorna `PortfolioValuationResponse`
- **Regra de Negócio:**
  - Converte cada item da lista para a moeda `targetCurrency`.
  - Calcula o `TotalValue` consolidado.
  - Calcula o percentual `SharePercentage` (0 a 100 com 2 casas decimais) de cada ativo em relação ao total da carteira para alimentar diretamente componentes de Gráfico de Pizza no frontend.

### Endpoint 3: Tendências Históricas (Time-Series)
- **Rota:** `GET /api/v1/exchange/history`
- **Query Params:**
  - `base` (string, ISO-3, ex: `USD`)
  - `target` (string, ISO-3, ex: `BRL`)
  - `period` (string: `7d`, `30d`, `90d` - default: `30d`)
- **Caso de Uso:** `GetHistoricalRatesQuery` -> Retorna `HistoricalRatesResponse`
- **Regra de Negócio:**
  - Calcula a janela temporal (`startDate` e `endDate`) a partir do período solicitado.
  - Consome o histórico via `IExchangeRateProvider` (protegido por cache de 24h no Redis).
  - Retorna um array ordenado cronologicamente de `{ "date": "YYYY-MM-DD", "rate": 0.000000 }` para consumo direto por Gráficos de Linha.

### Endpoint 4: Cotações em Tempo Real (Showcase de Performance)
- **Rota:** `GET /api/v1/exchange/latest`
- **Query Params:**
  - `base` (string, ISO-3, default: `USD`)
- **Caso de Uso:** `GetLatestRatesQuery` -> Retorna `LatestRatesResponse`
- **Regra de Negócio:** Retorna todas as moedas indexadas pela moeda base (protegido pelo cache de 15 minutos no Redis).

### Endpoint 5: Dicionário de Moedas (Long-lived Cache)
- **Rota:** `GET /api/v1/exchange/currencies`
- **Caso de Uso:** `GetCurrenciesQuery` -> Retorna `CurrenciesResponse`
- **Regra de Negócio:** Retorna pares chave/valor de moedas suportadas com nomes completos (cache de 7 dias no Redis).

### Endpoint 6: Auditoria de Transações (CQRS Read-Side)
- **Rota:** `GET /api/v1/exchange/audit`
- **Query Params:**
  - `take` (int, default: 50, max: 100)
- **Caso de Uso:** `GetAuditLogsQuery` -> Retorna `IReadOnlyList<AuditLogDto>`
- **Regra de Negócio:** Consulta diretamente o banco relacional PostgreSQL através de `IConversionAuditRepository` sem acionar provedores externos, comprovando o isolamento do lado de leitura.

---

## 3. Tratamento Global de Erros (RFC 7807)
Implementação de `IExceptionHandler` nativo do .NET 9 capturando exceções e convertendo em Problem Details:
- `DomainException` -> HTTP 400 Bad Request
- `ValidationException` -> HTTP 422 Unprocessable Entity
- `ExchangeIntegrationException` -> HTTP 502 Bad Gateway
- Erros não mapeados -> HTTP 500 Internal Server Error
- `BadHttpRequestException` (corpo JSON malformado, binding inválido) -> HTTP 400 Bad Request (incluído após o smoke test; ver seção 4).

---

## 4. Decisões de Implementação e Desvios (registro para ADRs)

- **MediatR fixado na linha 12.x:** as versões 13 e superiores passaram a exigir licença comercial. A 12.x é a última distribuída sob Apache-2.0.
- **Publicação síncrona da notificação:** `IPublisher.Publish` aguarda os handlers. A persistência da auditoria, portanto, soma latência à resposta da conversão. O handler de auditoria captura falhas de persistência e as registra em log, para que a conversão já calculada nunca seja invalidada. Alternativa futura para remover a latência: fila em memória com `Channel<T>` e `BackgroundService`, a ser decidida em ADR.
- **Registro de `ISpreadStrategyResolver`:** o serviço é registrado em `AddApplication`. A ausência desse registro foi detectada apenas no smoke test da API; por isso há um teste com `ValidateOnBuild = true` que resolve os handlers pelo contêiner.
- **Fatias percentuais por método dos maiores restos:** `SharePercentageCalculator` (Domain) distribui o resto de centésimos para que as fatias somem exatamente 100,00. Cada ativo é convertido com a taxa de referência, sem spread, e o total é a soma dos valores convertidos já arredondados em 2 casas.
- **Tratamento global de erros:** `GlobalExceptionHandler` implementa `IExceptionHandler` e usa `IProblemDetailsService`. Para erros 500, o detalhe não vaza a mensagem interna; a exceção é registrada em log.
- **Serialização:** `JsonStringEnumConverter` e `camelCase` configurados via `ConfigureHttpJsonOptions`.
- **Scalar apenas em Development:** `MapOpenApi` e `MapScalarApiReference` ficam dentro do bloco `IsDevelopment()`, mantendo a regra já existente para o OpenAPI. Não expor a documentação em produção é uma decisão de segurança a confirmar.
- **Endpoints como Minimal API:** o mapeamento fica em `Endpoints/ExchangeEndpoints.cs`, agrupado em `/api/v1/exchange`, em vez de controllers. O parâmetro `base` é mapeado com `[FromQuery(Name = "base")]` porque `base` é palavra reservada em C#.
- **Limitações conhecidas:** os endpoints não têm teste de integração; o fluxo feliz de `POST /portfolio/valuate` depende de cotações reais e foi coberto apenas por testes unitários dos handlers.