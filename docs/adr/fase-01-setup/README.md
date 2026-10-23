# Fase 01 - Setup da Solution e Clean Architecture

## Escopo

Criação da solution .NET 9 com a estrutura da Clean Architecture, configuração do repositório e infraestrutura local com Docker Compose.

## O que foi implementado

- `global.json` fixando o SDK em `9.0.318`.
- `SmartExchange.sln` com os projetos:
  - `src/SmartExchange.Api` (Web API, minimal hosting)
  - `src/SmartExchange.Application` (class library)
  - `src/SmartExchange.Domain` (class library, sem dependências externas)
  - `src/SmartExchange.Infrastructure` (class library)
  - `tests/SmartExchange.Tests.Unit` (xUnit)
- Referências entre projetos conforme a Clean Architecture (ver ADR-002).
- `docker-compose.yml` com `postgres:16-alpine` e `redis:7-alpine`.
- `.dockerignore` e `.gitignore` (padrão .NET).

## Decisões

| ADR | Título | Status |
|---|---|---|
| [ADR-001](ADR-001-sdk-pin-global-json.md) | Fixar o SDK do .NET 9 com `global.json` | Aceito |
| [ADR-002](ADR-002-clean-architecture-references.md) | Dependências unidirecionais entre projetos | Aceito |
| [ADR-003](ADR-003-build-output-fora-do-versionamento.md) | Build output fora do versionamento | Aceito |
| [ADR-004](ADR-004-versoes-das-bibliotecas-de-teste.md) | Versões das bibliotecas de teste | Aceito |

## Desvios registrados

- A porta do PostgreSQL no host foi alterada para `5434` na Fase 04 (ADR-015).
