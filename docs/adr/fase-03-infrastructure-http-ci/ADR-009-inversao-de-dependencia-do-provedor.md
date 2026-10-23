# ADR-009: Contrato `IExchangeRateProvider` na Application, implementação na Infrastructure

- **Status:** Aceito
- **Fase:** 03 - Infraestrutura HTTP

## Contexto

Os casos de uso precisam de cotações, mas não devem conhecer HTTP, URLs ou formato JSON da Frankfurter. Se o cliente HTTP fosse referenciado diretamente pela Application, a troca de provedor ou a introdução de cache exigiria mudar a camada de aplicação.

## Decisão

- `IExchangeRateProvider` é declarada em `SmartExchange.Application/Common/Interfaces`, usando apenas tipos do Domain (`Currency`, `ExchangeRate`).
- `FrankfurterApiClient` implementa a interface em `SmartExchange.Infrastructure`.
- A Application nunca referencia a Infrastructure. A ligação acontece na composição, feita pela Api.

## Consequências

- O cache (Fase 04) e os fakes de teste (Fase 06) entram como decoradores ou substitutos da mesma interface, sem tocar nos casos de uso.
- Os DTOs da Frankfurter ficam isolados em `Infrastructure/Http/Dtos`; mudanças no JSON do provedor não atravessam a fronteira.
