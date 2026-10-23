# ADR-022: Frankfurter substituída por fake nos testes de integração

- **Status:** Aceito
- **Fase:** 06 - Docker e Testes de Integração

## Contexto

A constitution da Fase 06 exige que os testes de integração sejam determinísticos e não dependam de serviços externos. A Frankfurter API é externa: depende de internet, pode mudar as cotações entre execuções e pode ficar indisponível. Um teste ponta a ponta que chama a API real teria resultados diferentes a cada execução.

## Decisão

`FakeFrankfurterHandler` (`HttpMessageHandler`) é injetado como handler primário do `FrankfurterApiClient` por `ConfigurePrimaryHttpMessageHandler`, em `ConfigureTestServices`. Ele responde com cotações fixas para os pares usados pelos testes (`USD→BRL`, `EUR→USD`) e com a lista de moedas.

Continuam reais, nos testes de integração:

- PostgreSQL (Testcontainers)
- Redis (Testcontainers)
- Decorator de cache, com gravação e leitura no Redis
- Pipeline de resiliência, serialização e tratamento de erros
- Persistência da auditoria

## Consequências

- Os valores esperados são calculados à mão e não mudam entre execuções (ex.: `100 USD × 5,00 + 1,5% = 507,50`).
- A suíte roda sem internet.
- O contrato real com a Frankfurter não é testado por esta suíte. Uma mudança de formato do provedor seria detectada apenas em produção ou em teste manual. Isso é um risco aceito, mitigado pelo fato de os DTOs estarem isolados (ADR-009).
- Como o fake conta requisições, o teste de cache mede que a segunda chamada não chega ao provedor.
