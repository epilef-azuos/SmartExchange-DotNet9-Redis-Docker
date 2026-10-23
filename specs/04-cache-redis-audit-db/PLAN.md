# PLAN: Fase 04

1. **Branch e Instalação de Pacotes**
   - Criar e chavear para a branch `feature/04-cache-redis-audit-db`.
   - Adicionar `Microsoft.Extensions.Caching.StackExchangeRedis` e `Npgsql.EntityFrameworkCore.PostgreSQL` em `SmartExchange.Infrastructure`.
   - Adicionar `Microsoft.EntityFrameworkCore.Design` em `SmartExchange.Api`.
2. **Modelagem de Domínio e Contratos de Persistência**
   - Criar entidade `ConversionAuditLog` em `SmartExchange.Domain/Entities/`.
   - Criar interface `IConversionAuditRepository` em `SmartExchange.Application/Common/Interfaces/`.
3. **Implementação do Decorator Pattern de Cache**
   - Implementar `CachedExchangeRateProvider` em `SmartExchange.Infrastructure/Caching/`.
   - Implementar geração estruturada de chaves (`CacheKeys.cs`).
   - Configurar registro no contêiner de DI em `SmartExchange.Infrastructure/DependencyInjection.cs`.
4. **Implementação do EF Core e Repositório de Auditoria**
   - Implementar `AuditDbContext` em `SmartExchange.Infrastructure/Persistence/`.
   - Configurar Fluent API em `SmartExchange.Infrastructure/Persistence/Configurations/ConversionAuditLogConfiguration.cs`.
   - Implementar `ConversionAuditRepository` em `SmartExchange.Infrastructure/Persistence/Repositories/`.
   - Registrar `AuditDbContext` e `IConversionAuditRepository` na injeção de dependência.
   - Gerar a migration `InitialConversionAudit` (`dotnet ef migrations add ... --context AuditDbContext`), com a Infrastructure como projeto e a Api como startup.
5. **Testes Unitários**
   - Criar `CachedExchangeRateProviderTests.cs` em `tests/SmartExchange.Tests.Unit/Infrastructure/Caching/` mockando `IDistributedCache` e `IExchangeRateProvider`.
6. **Validação e Compilação**
   - Executar `dotnet build` e `dotnet test`.
7. **Ajustes de Ambiente (pós-implementação)**
   - Remapear o PostgreSQL do host para a porta 5434, por conflito com o PostgreSQL 15 nativo na 5432 (ver SPEC, seção 4).
   - Recriar os containers (`docker compose down` seguido de `docker compose up -d`) e aplicar a migration com `dotnet ef database update`.