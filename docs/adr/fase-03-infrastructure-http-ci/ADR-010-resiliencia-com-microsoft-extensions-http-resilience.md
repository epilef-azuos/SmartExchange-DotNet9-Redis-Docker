# ADR-010: Resiliência com `Microsoft.Extensions.Http.Resilience`

- **Status:** Aceito
- **Fase:** 03 - Infraestrutura HTTP

## Contexto

A Frankfurter pode responder com erro transitório (5xx, 503) ou demorar. Uma falha momentânea não deve derrubar a conversão, mas também não deve manter o cliente preso por tempo indefinido nem insistir sobre um provedor fora do ar.

## Decisão

Usar o pipeline nativo do .NET 9 (`Microsoft.Extensions.Http.Resilience`), sem bibliotecas legadas de encapsulamento manual, configurado em `ConfigureFrankfurterResilience`:

| Componente | Valor | Justificativa |
|---|---|---|
| Retry | 3 tentativas, backoff exponencial com jitter | Absorve falhas transitórias sem sincronizar as tentativas de vários clientes |
| Timeout por tentativa | 5 s | Limita cada chamada individual |
| Timeout total | 45 s | Ajuste próprio: com 4 tentativas e backoff, o valor de 30 s padrão cortaria as retentativas |
| Circuit breaker | 50% de falhas em janela de 10 s | Valores definidos na spec |
| `MinimumThroughput` | 5 | Ajuste próprio: o padrão de 100 requisições impediria o breaker de abrir em baixo volume |

## Consequências

- Um erro 500 persistente gera 4 chamadas no total (1 + 3 retries) antes de virar `ExchangeIntegrationException` com `StatusCode = 500`. Os testes verificam essa contagem.
- Quando o circuito abre, as chamadas falham rapidamente sem tocar na Frankfurter. Essa exceção do Polly não é convertida em `ExchangeIntegrationException`: é um ponto em aberto, a tratar se o contrato de erro precisar ser estrito.
- O delay entre tentativas usa o padrão da biblioteca (2 s base). Nos testes, o delay é reduzido para não tornar a suíte lenta.
