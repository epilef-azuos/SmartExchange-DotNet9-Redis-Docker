# SPEC: Fase 04 - Cache Distribuído com Redis e Persistência de Auditoria com EF Core / PostgreSQL

## Objetivo
Configurar o cache distribuído com Redis implementando o Decorator Pattern sobre `IExchangeRateProvider`, garantindo TTLs diferenciados e fallback gracioso, além de configurar o Entity Framework Core com PostgreSQL para armazenar o histórico imutável das conversões cambiais.

## Requisitos Técnicos

### 1. Cache Distribuído com Redis (`SmartExchange.Infrastructure`)
- **Pacote:** `Microsoft.Extensions.Caching.StackExchangeRedis` (v9.x).
- **Decorator `CachedExchangeRateProvider`:**
  - Implementa `IExchangeRateProvider` e recebe uma instância interna de `IExchangeRateProvider` (o cliente real) e `IDistributedCache`.
  - Serialização/Desserialização via `System.Text.Json` com suporte a tipos complexos e record structs.
  - **Chaves de Cache padronizadas:**
    - `exchange:latest:{base}` -> TTL: 15 min.
    - `exchange:pair:{base}:{target}` -> TTL: 15 min.
    - `exchange:currencies` -> TTL: 7 dias.
    - `exchange:history:{base}:{target}:{start}:{end}` -> TTL: 24 horas.
- **Injeção de Dependência:** Registro do Decorator no contêiner de DI sem pacotes de terceiros, aproveitando a tipagem concreta do client ou Keyed Services do .NET.

### 2. Persistência e Log de Auditoria (`SmartExchange.Application` & `SmartExchange.Infrastructure`)
- **Pacotes:**
  - `Npgsql.EntityFrameworkCore.PostgreSQL` (v9.x) no projeto `SmartExchange.Infrastructure`.
  - `Microsoft.EntityFrameworkCore.Design` (v9.x) no projeto `SmartExchange.Api` (startup).
- **Entidade de Auditoria (`SmartExchange.Domain/Entities/ConversionAuditLog.cs`):**
  - `Guid Id` (PK)
  - `string ClientTier`
  - `string SourceCurrency` (ISO 3 letras)
  - `string TargetCurrency` (ISO 3 letras)
  - `decimal BaseAmount`
  - `decimal ConvertedAmount`
  - `decimal BaseRate`
  - `decimal FinalRateWithSpread`
  - `decimal SpreadPercentage`
  - `decimal SpreadAmount`
  - `DateTimeOffset CreatedAtUtc`
- **Contrato na Application (`IConversionAuditRepository.cs`):**
  - `Task AddAsync(ConversionAuditLog log, CancellationToken ct = default);`
  - `Task<IReadOnlyList<ConversionAuditLog>> GetRecentAuditsAsync(int take = 50, CancellationToken ct = default);`
- **Contexto EF Core (`AuditDbContext.cs`):**
  - Mapeamento explícito via Fluent API (`ConversionAuditLogConfiguration.cs`).
  - Colunas `decimal` mapeadas explicitamente com `HasPrecision(18, 6)` para taxas e `HasPrecision(18, 2)` para quantias.
  - Tabela: `conversion_audit_logs`.
  - Migration inicial: `20261023002612_InitialConversionAudit` (pasta `src/SmartExchange.Infrastructure/Persistence/Migrations/`), aplicada ao PostgreSQL local.

### 4. Ambiente Local e Portas (ajuste pós-implementação)
- Serviços em `docker-compose.yml`: `postgres` e `redis`.
- **PostgreSQL:** mapeado para a porta do host **5434** (`"5434:5432"`); a porta interna do container permanece 5432.
  - Motivo: o Windows de desenvolvimento possui instalações nativas do PostgreSQL (15 na porta 5432 e 18 na porta 5433). Com o mapeamento `5432:5432`, `localhost:5432` era atendido pelo PostgreSQL 15 nativo, que não possui a role `smartexchange`, resultando em `28000: role "smartexchange" does not exist`.
- **Redis:** mantido em `6379:6379`, sem conflito no host.
- Connection strings (`src/SmartExchange.Api/appsettings.json`, seção `ConnectionStrings`):
  - `Redis`: `localhost:6379`
  - `AuditDb`: `Host=localhost;Port=5434;Database=smartexchange;Username=smartexchange;Password=smartexchange` (credenciais apenas para desenvolvimento local).

### 5. Decisões de Implementação
- **Decorator sem alterar o cliente HTTP:** `FrankfurterApiClient` é registrado pelo tipo concreto (`AddHttpClient<FrankfurterApiClient>`). `IExchangeRateProvider` é registrado como `CachedExchangeRateProvider`, que recebe o cliente concreto por construtor. Nenhuma alteração no cliente foi necessária.
- **Assinatura de composição:** `AddInfrastructure(IServiceCollection, IConfiguration)` é o ponto único de registro da Infrastructure. As opções do Frankfurter são vinculadas com `Bind(configuration.GetSection(...))` dentro dele, para não depender de `IConfiguration` já registrado no contêiner.
- **Tolerância a falhas de cache:** `IDistributedCache` é acessado dentro de `try/catch` que ignora apenas `OperationCanceledException`. Falhas de leitura ou de escrita são registradas com `LogWarning` e a cotação segue da origem. Entrada corrompida é tratada como cache miss.
- **Precisão decimal:** quantias monetárias usam `HasPrecision(18, 2)`; taxas e percentuais usam `HasPrecision(18, 6)`. A regra é verificada por testes sobre o modelo do EF, sem conexão com o banco.
- **Entidade append-only:** `ConversionAuditLog` não tem setters públicos, é criada por `FromConversion(id, result, createdAtUtc)` e o `IConversionAuditRepository` não expõe atualização nem remoção.
- **TTLs** fixados no decorator: pares e `latest` em 15 min, histórico em 24 h e moedas em 7 dias.

### 6. Limitações Conhecidas
- Escrita e leitura no PostgreSQL não têm teste unitário; dependem de um banco real e devem ser cobertas por teste de integração.
- Nenhum caso de uso grava a auditoria ainda, porque o endpoint de conversão não existe nesta fase.
- O comportamento do Redis é verificado com mocks de `IDistributedCache`, não com um Redis em execução.

### 3. Testes Unitários
- Validação do `CachedExchangeRateProvider`:
  - Cenário de Cache Hit (não chama o provedor interno).
  - Cenário de Cache Miss (chama o provedor interno e grava no Redis com o TTL correspondente).
  - Cenário de falha no Redis (fallback para a chamada HTTP sem quebrar a requisição).