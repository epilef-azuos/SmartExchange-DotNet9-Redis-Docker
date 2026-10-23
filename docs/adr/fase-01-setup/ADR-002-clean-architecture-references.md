# ADR-002: Dependências unidirecionais entre projetos, com Domain sem pacotes

- **Status:** Aceito
- **Fase:** 01 - Setup da Solution

## Contexto

A solução precisa isolar a regra de negócio de detalhes de infraestrutura (HTTP, banco, cache) e de apresentação, para que a regra seja testável sem dependências externas.

## Decisão

Grafo de referências, sempre apontando para dentro:

```
Api ──► Application ──► Domain
 │            ▲
 └──► Infrastructure ──► Application
```

- `Domain` não referencia nenhum projeto nem pacote NuGet de produção.
- `Application` referencia apenas `Domain` (e, a partir da Fase 05, MediatR e abstrações de logging).
- `Infrastructure` referencia `Application` e implementa suas abstrações.
- `Api` referencia `Application` e `Infrastructure` apenas para compor a injeção de dependência.
- Os testes unitários referenciam `Domain`, `Application` e `Infrastructure`.

## Consequências

- A regra de negócio pode ser testada sem Docker, rede ou banco.
- Uma violação do grafo (por exemplo, `Domain` referenciando EF Core) quebra a compilação ou aparece na revisão de PR.
- A composição (DI) fica concentrada na `Api`, que é o único projeto que conhece todas as camadas em tempo de execução.
