# Architecture Decision Records (ADR)

Registro das decisões técnicas do SmartExchange Gateway, organizado por fase de implementação.
Cada decisão relevante tem um arquivo próprio, numerado em ordem cronológica e imutável após aceita:
para mudar uma decisão, cria-se uma nova ADR que a substitui e atualiza-se o status da antiga.

## Status possíveis

| Status | Significado |
|---|---|
| Proposto | Em discussão, ainda não implementado. |
| Aceito | Decisão vigente e implementada. |
| Substituído | Superado por outra ADR (indicada no próprio arquivo). |

## Modelo

```markdown
# ADR-NNN: Título curto da decisão

- **Status:** Proposto | Aceito | Substituído por ADR-XXX
- **Fase:** NN - nome da fase

## Contexto
Problema, restrições e forças em jogo.

## Decisão
O que foi decidido, em uma ou duas frases objetivas.

## Consequências
Efeitos positivos, custos e riscos assumidos.
```

## Índice por fase

### Fase 01 - Setup e Clean Architecture
Pasta: [fase-01-setup](fase-01-setup/README.md)

- [ADR-001](fase-01-setup/ADR-001-sdk-pin-global-json.md): Fixar o SDK do .NET 9 com `global.json`
- [ADR-002](fase-01-setup/ADR-002-clean-architecture-references.md): Dependências unidirecionais entre projetos, com Domain sem pacotes
- [ADR-003](fase-01-setup/ADR-003-build-output-fora-do-versionamento.md): Build output fora do versionamento
- [ADR-004](fase-01-setup/ADR-004-versoes-das-bibliotecas-de-teste.md): Versões das bibliotecas de teste

### Fase 02 - Domínio (Value Objects e Spread)
Pasta: [fase-02-domain-logic](fase-02-domain-logic/README.md)

- [ADR-005](fase-02-domain-logic/ADR-005-currency-como-record-class.md): `Currency` como `sealed record`, e não `readonly record struct`
- [ADR-006](fase-02-domain-logic/ADR-006-arredondamento-financeiro.md): Arredondamento explícito com `MidpointRounding.ToEven` (6 casas em taxas, 2 em quantias)
- [ADR-007](fase-02-domain-logic/ADR-007-money-rejeita-valores-negativos.md): `Money` rejeita valores negativos e operações entre moedas diferentes
- [ADR-008](fase-02-domain-logic/ADR-008-strategy-pattern-para-spread.md): Strategy Pattern para spread, com resolução por `ClientTier`

### Fase 03 - Infraestrutura HTTP, Resiliência e CI
Pasta: [fase-03-infrastructure-http-ci](fase-03-infrastructure-http-ci/README.md)

- [ADR-009](fase-03-infrastructure-http-ci/ADR-009-inversao-de-dependencia-do-provedor.md): Contrato `IExchangeRateProvider` na Application, implementação na Infrastructure
- [ADR-010](fase-03-infrastructure-http-ci/ADR-010-resiliencia-com-microsoft-extensions-http-resilience.md): Resiliência com `Microsoft.Extensions.Http.Resilience`
- [ADR-011](fase-03-infrastructure-http-ci/ADR-011-host-legado-da-frankfurter-api.md): Host legado `api.frankfurter.app` da Frankfurter API
- [ADR-012](fase-03-infrastructure-http-ci/ADR-012-pipeline-github-actions.md): Pipeline de CI no GitHub Actions

### Fase 04 - Cache Redis e Auditoria com EF Core
Pasta: [fase-04-cache-redis-audit-db](fase-04-cache-redis-audit-db/README.md)

- [ADR-013](fase-04-cache-redis-audit-db/ADR-013-cache-como-decorator.md): Cache como Decorator, sem alterar o cliente HTTP
- [ADR-014](fase-04-cache-redis-audit-db/ADR-014-auditoria-append-only-com-ef-core.md): Auditoria append-only com EF Core e precisão decimal explícita
- [ADR-015](fase-04-cache-redis-audit-db/ADR-015-postgresql-na-porta-5434.md): PostgreSQL do Docker publicado na porta 5434 do host

### Fase 05 - Aplicação CQRS, API e Erros RFC 7807
Pasta: [fase-05-application-cqrs-api](fase-05-application-cqrs-api/README.md)

- [ADR-016](fase-05-application-cqrs-api/ADR-016-mediatr-12-e-auditoria-por-notificacao.md): MediatR 12 e auditoria desacoplada por notificação
- [ADR-017](fase-05-application-cqrs-api/ADR-017-fatias-percentuais-pelo-metodo-dos-maiores-restos.md): Fatias percentuais pelo método dos maiores restos
- [ADR-018](fase-05-application-cqrs-api/ADR-018-erros-rfc-7807-com-iexceptionhandler.md): Erros em RFC 7807 com `IExceptionHandler`
- [ADR-019](fase-05-application-cqrs-api/ADR-019-endpoints-minimal-api-e-scalar-em-development.md): Endpoints em Minimal API e Scalar restrito a Development

### Fase 06 - Docker, Orquestração e Testcontainers
Pasta: [fase-06-docker-testcontainers](fase-06-docker-testcontainers/README.md)

- [ADR-020](fase-06-docker-testcontainers/ADR-020-dockerfile-multi-stage-com-usuario-app.md): Dockerfile multi-stage com usuário não-root
- [ADR-021](fase-06-docker-testcontainers/ADR-021-testcontainers-4-0-0-por-compatibilidade-do-docker.md): Testcontainers 4.0.0 por compatibilidade com a API do Docker
- [ADR-022](fase-06-docker-testcontainers/ADR-022-frankfurter-substituida-nos-testes-de-integracao.md): Frankfurter substituída por fake nos testes de integração
- [ADR-023](fase-06-docker-testcontainers/ADR-023-migrations-no-startup-da-api.md): Aplicação de migrations no startup da API (**Proposto**)
- [ADR-024](fase-06-docker-testcontainers/ADR-024-alinhamento-de-credenciais-e-connection-strings.md): Alinhamento de credenciais e chaves de connection string no compose
