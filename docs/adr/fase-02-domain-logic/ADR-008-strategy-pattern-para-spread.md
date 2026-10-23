# ADR-008: Strategy Pattern para spread, com resolução por `ClientTier`

- **Status:** Aceito
- **Fase:** 02 - Domínio

## Contexto

O spread cambial depende do perfil do cliente (`Standard`, `Premium`, `Vip`). Novos perfis ou novas regras de spread não devem exigir alterar o cálculo existente.

## Decisão

- Interface `ISpreadStrategy` com `Tier`, `SpreadPercentage`, `CalculateSpreadAmount` e `ApplySpreadToRate`.
- `SpreadStrategyBase` (abstrata) concentra a lógica comum de cálculo e validação (montante não negativo, taxa maior que zero). As três estratégias concretas apenas informam o perfil e o percentual.
- `SpreadStrategyResolver` escolhe a estratégia por `switch` sobre o `ClientTier`. Um valor fora do enum lança `DomainException`.
- Os percentuais são constantes: 3,5%, 1,5% e 0,5%.

## Alternativas consideradas

- Mapa de dicionário preenchido por DI: rejeitado na Fase 02 porque o domínio não depende de DI (ADR-002). Na Fase 05 o resolver passou a ser registrado no contêiner, sem alterar o contrato.

## Consequências

- Adicionar um perfil exige uma nova classe e um novo ramo no `switch`. O compilador não obriga o ramo, mas o teste do resolver cobre o valor indefinido.
- O cálculo é puro e determinístico, o que facilita os testes de precisão decimal.
