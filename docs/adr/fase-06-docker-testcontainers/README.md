# Fase 06 - Docker, Orquestração e Testes de Integração

## Escopo

Empacotamento da API em imagem Docker multi-stage, orquestração local com healthchecks (API, PostgreSQL e Redis) e suíte de integração ponta a ponta com contêineres descartáveis via Testcontainers.

## O que foi implementado

- **Dockerfile multi-stage:** restore isolado, publicação, e runtime `aspnet:9.0-alpine` com `USER app`.
- **`docker-compose.yml`:** serviços `postgres`, `redis` e `api`, com healthchecks e `depends_on` com `condition: service_healthy`.
- **Projeto `tests/SmartExchange.Tests.Integration`:** `SmartExchangeApiFactory` (`WebApplicationFactory<Program>` + `IAsyncLifetime`), que sobe PostgreSQL e Redis descartáveis com portas dinâmicas e aplica as migrations.
- **Fake da Frankfurter:** `FakeFrankfurterHandler`, que substitui apenas o handler HTTP do cliente.
- **Cinco testes de integração** cobrindo conversão com auditoria, persistência física no PostgreSQL, erro 400 em RFC 7807, carteira com fatias e cache do `currencies` no Redis.

## Decisões

| ADR | Título | Status |
|---|---|---|
| [ADR-020](ADR-020-dockerfile-multi-stage-com-usuario-app.md) | Dockerfile multi-stage com usuário não-root | Aceito |
| [ADR-021](ADR-021-testcontainers-4-0-0-por-compatibilidade-do-docker.md) | Testcontainers 4.0.0 por compatibilidade com a API do Docker | Aceito |
| [ADR-022](ADR-022-frankfurter-substituida-nos-testes-de-integracao.md) | Frankfurter substituída por fake nos testes de integração | Aceito |
| [ADR-023](ADR-023-migrations-no-startup-da-api.md) | Aplicação de migrations no startup da API | **Proposto** |
| [ADR-024](ADR-024-alinhamento-de-credenciais-e-connection-strings.md) | Alinhamento de credenciais e chaves de connection string | Aceito |

## Como executar

Pré-requisito: Docker em execução.

```bash
# Subir API, PostgreSQL e Redis
docker compose up --build -d

# Testes unitários e de integração (a integração sobe contêineres próprios)
dotnet test SmartExchange.sln
```

Portas no host: API em `5000`, PostgreSQL em `5434` (ver ADR-015 da Fase 04), Redis em `6379`. Com `ASPNETCORE_ENVIRONMENT=Development`, a documentação está em `http://localhost:5000/scalar/v1`.

## Atenção

A API não cria o schema do banco sozinha. Em um ambiente novo, aplique a migration antes de usar a auditoria (ver ADR-023).
