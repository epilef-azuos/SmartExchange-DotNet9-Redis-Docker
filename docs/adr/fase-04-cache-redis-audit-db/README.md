# Fase 04 - Cache Distribuído (Redis) e Auditoria (EF Core + PostgreSQL)

## Escopo

Cache distribuído das cotações com TTLs diferenciados por volatilidade, e persistência append-only do histórico de conversões em PostgreSQL.

## O que foi implementado

- **Cache:** `CachedExchangeRateProvider` (Decorator sobre `IExchangeRateProvider`), `CacheKeys` com as chaves padronizadas e registro em `Infrastructure/DependencyInjection.cs`.
- **TTLs:** pares e `latest` em 15 min, histórico em 24 h, moedas em 7 dias.
- **Redis:** `Microsoft.Extensions.Caching.StackExchangeRedis` com `IDistributedCache`.
- **Persistência:** entidade `ConversionAuditLog` (domínio), `IConversionAuditRepository` (Application), `AuditDbContext`, mapeamento Fluent API `ConversionAuditLogConfiguration` e `ConversionAuditRepository`.
- **Migration:** `20261023002612_InitialConversionAudit`, aplicada no PostgreSQL local (porta 5434).

## Decisões

| ADR | Título | Status |
|---|---|---|
| [ADR-013](ADR-013-cache-como-decorator.md) | Cache como Decorator, sem alterar o cliente HTTP | Aceito |
| [ADR-014](ADR-014-auditoria-append-only-com-ef-core.md) | Auditoria append-only com EF Core e precisão decimal explícita | Aceito |
| [ADR-015](ADR-015-postgresql-na-porta-5434.md) | PostgreSQL do Docker publicado na porta 5434 do host | Aceito |

## Operação

Para aplicar a migration em um banco novo:

```bash
dotnet ef database update --project src/SmartExchange.Infrastructure --startup-project src/SmartExchange.Api --context AuditDbContext
```
