# ADR-017: Fatias percentuais pelo método dos maiores restos

- **Status:** Aceito
- **Fase:** 05 - Aplicação (CQRS)

## Contexto

O endpoint de valoração de carteira devolve a fatia (`SharePercentage`) de cada ativo, para alimentar gráficos de pizza. Arredondar cada fatia isoladamente para 2 casas pode fazer a soma ficar em 99,99% ou 100,01%, o que aparece como erro no gráfico.

## Decisão

`SharePercentageCalculator` (Domain) usa o método dos maiores restos:

1. Calcula cada fatia em centésimos de ponto percentual (`valor × 10000 ÷ total`).
2. Trunca cada valor para inteiro e guarda o resto fracionário.
3. Distribui a diferença até 10000 somando 1 aos ativos de maior resto (desempate pela ordem de entrada).
4. Converte de volta para percentual com 2 casas.

Regras de borda: total zero devolve todas as fatias zeradas.

Antes do cálculo, cada ativo é convertido para a moeda alvo com a taxa de referência, **sem spread**, e o total é a soma desses valores já arredondados em 2 casas.

## Consequências

- Para qualquer carteira com total positivo, as fatias somam exatamente 100,00. Os testes verificam isso com conjuntos arbitrários.
- A diferença máxima entre a fatia exata e a apresentada é de 0,01 ponto percentual, e vai sempre para o ativo que mais perderia no arredondamento.
- Valorar a carteira sem spread é uma escolha de apresentação: o valor é de referência, não um preço de execução. Se o produto precisar de valoração com spread, isso é uma mudança de regra a ser registrada.
