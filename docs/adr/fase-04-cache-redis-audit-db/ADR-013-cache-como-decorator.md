# ADR-013: Cache como Decorator, sem alterar o cliente HTTP

- **Status:** Aceito
- **Fase:** 04 - Cache e Auditoria

## Contexto

As cotações podem ser cacheadas no Redis para reduzir chamadas à Frankfurter e a latência das conversões. A lógica de cache não deve misturar-se com a de chamada HTTP, e o cliente não deve saber que existe um cache.

## Decisão

- `CachedExchangeRateProvider` implementa `IExchangeRateProvider` e recebe por construtor o `FrankfurterApiClient` concreto, mais `IDistributedCache` e o logger.
- O cliente é registrado pelo tipo concreto (`AddHttpClient<FrankfurterApiClient>`). Quem resolve `IExchangeRateProvider` recebe o decorator, e o cliente não sabe que existe cache.
- Padrão cache-aside: lê o cache; em miss, chama o provedor, grava com TTL e devolve o valor.
- **Tolerância a falhas:** exceções de leitura e de escrita no Redis são capturadas (exceto `OperationCanceledException`), registradas com `LogWarning`, e a cotação segue da origem. Uma entrada corrompida é tratada como miss.
- Erros do provedor (`ExchangeIntegrationException`) **não** são capturados pelo decorator; propagam normalmente.
- TTLs: `latest` e `pair` 15 min; `history` 24 h (dados passados são imutáveis); `currencies` 7 dias.

## Alternativas consideradas

- Scrutor ou decorator via `Keyed Services`: rejeitado para não adicionar pacote de terceiros; o registro manual com factory é suficiente para um decorator.

## Consequências

- Um Redis indisponível degrada para chamadas diretas à Frankfurter, em vez de derrubar a conversão. O custo é latência maior enquanto o Redis estiver fora.
- As cotações usadas no cache podem ter até 15 min de defasagem. Isso é aceitável para o perfil de uso e deve ser comunicado ao consumidor da API.
- Os testes do decorator usam `IDistributedCache` mockado e verificam hit, miss com TTL, falha de leitura, falha de escrita, entrada corrompida e cancelamento.
