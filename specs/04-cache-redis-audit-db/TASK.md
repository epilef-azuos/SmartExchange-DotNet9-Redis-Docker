# TASK: Cache Distribuído com Redis (Decorator Pattern) e Persistência de Auditoria com EF Core

Status Geral: CONCLUÍDA (código, testes e migration aplicada; commit pendente)
Fase: 04 - Cache Redis & Audit DB
Branch: feature/04-cache-redis-audit-db

---

## 1. Controle de Versão e Dependências
- [x] Criar e chavear para a branch `feature/04-cache-redis-audit-db`.
- [x] Adicionar pacote `Microsoft.Extensions.Caching.StackExchangeRedis` em `src/SmartExchange.Infrastructure`.
- [x] Adicionar pacote `Npgsql.EntityFrameworkCore.PostgreSQL` em `src/SmartExchange.Infrastructure`.
- [x] Adicionar pacote `Microsoft.EntityFrameworkCore.Design` em `src/SmartExchange.Api`.

## 2. Entidade de Domínio e Contrato de Persistência
- [x] Criar entidade `ConversionAuditLog.cs` em `src/SmartExchange.Domain/Entities/`.
- [x] Criar interface `IConversionAuditRepository.cs` em `src/SmartExchange.Application/Common/Interfaces/`.

## 3. Implementação do Cache Distribuído (Decorator Pattern)
- [x] Criar classe estática `CacheKeys.cs` em `src/SmartExchange.Infrastructure/Caching/` para padronizar formatos de chaves.
- [x] Implementar `CachedExchangeRateProvider.cs` em `src/SmartExchange.Infrastructure/Caching/` encapsulando as regras de TTL (15m, 24h, 7d) e fallback resiliente em caso de falha do Redis.
- [x] Configurar injeção de dependência via Decorator Pattern para `IExchangeRateProvider` em `src/SmartExchange.Infrastructure/DependencyInjection.cs`.

## 4. Persistência de Dados e Repositório (EF Core + PostgreSQL)
- [x] Implementar mapeamento Fluent API `ConversionAuditLogConfiguration.cs` com precisão decimal em `src/SmartExchange.Infrastructure/Persistence/Configurations/`.
- [x] Implementar `AuditDbContext.cs` em `src/SmartExchange.Infrastructure/Persistence/`.
- [x] Implementar `ConversionAuditRepository.cs` em `src/SmartExchange.Infrastructure/Persistence/Repositories/`.
- [x] Registrar `AuditDbContext` e `IConversionAuditRepository` no arquivo de extensão de DI da infraestrutura.

## 5. Testes Unitários de Cache
- [x] Criar testes unitários em `tests/SmartExchange.Tests.Unit/Infrastructure/Caching/CachedExchangeRateProviderTests.cs` cobrindo:
  - Cache Hit retornando valor sem invocar o client interno.
  - Cache Miss invocando o client interno e gravando no cache com expiração.
  - Falha transitória de serialização/Redis retornando a resposta da Frankfurter API sem lançar exceção.

## 6. Validação de Build e Rastreabilidade
- [x] Executar `dotnet build` garantindo 0 warnings e 0 errors.
- [x] Executar `dotnet test` assegurando 100% de aprovação na suíte de testes.
- [x] Marcar todos os checkboxes deste `TASK.md` como `[x]`.

## 7. Ajustes de Ambiente e Migration (pós-implementação)
- [x] Gerar a migration `20261023002612_InitialConversionAudit` para o `AuditDbContext`.
- [x] Remapear o PostgreSQL do host para a porta `5434` (`docker-compose.yml`) e atualizar `AuditDb` no `appsettings.json`.
- [x] Recriar os containers com `docker compose down` e `docker compose up -d`.
- [x] Aplicar a migration com `dotnet ef database update` e verificar a tabela `conversion_audit_logs` no banco.