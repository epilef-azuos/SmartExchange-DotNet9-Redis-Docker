# ADR-019: Endpoints em Minimal API e Scalar restrito a Development

- **Status:** Aceito
- **Fase:** 05 - Aplicação (CQRS)

## Contexto

A spec aceita controllers ou endpoints de extensão. A documentação interativa com Scalar expõe o contrato completo da API e não deve ficar disponível publicamente em produção por padrão.

## Decisão

- Endpoints como **Minimal API**, agrupados em `MapGroup("/api/v1/exchange")` e definidos em `Api/Endpoints/ExchangeEndpoints.cs`.
- Cada endpoint apenas traduz a requisição em um comando ou consulta e envia pelo `ISender`. Não há regra de negócio na camada HTTP.
- `MapOpenApi` e `MapScalarApiReference` ficam dentro do bloco `IsDevelopment()`, mantendo a regra já existente para o OpenAPI.
- O parâmetro de query `base` é mapeado com `[FromQuery(Name = "base")]`, porque `base` é palavra reservada em C#.
- Parâmetros opcionais têm padrão no endpoint (`tier` assume `Standard`, `take` assume 50, `period` assume 30d), e a validação dos valores é feita no handler.

## Consequências

- Em produção, a documentação interativa não é exposta. Quem precisar dela em outro ambiente deve habilitar explicitamente, em ADR própria.
- Os endpoints não têm controllers, o que reduz cerimônia, mas exige que o nome de cada rota seja definido com `WithName` para aparecer corretamente no OpenAPI.
- Os endpoints não têm teste de integração próprio; o fluxo principal é coberto pela suíte da Fase 06.
