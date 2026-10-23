# SPEC: Fase 06 - Docker Multi-Stage, Orquestração Local e Testes de Integração

## Objetivo
Implementar o Dockerfile multi-stage otimizado da API, unificar a orquestração local (API + Redis + PostgreSQL) no Docker Compose com verificações de saúde e criar a suíte de testes de integração com Testcontainers e `WebApplicationFactory`.

---

## 1. Dockerfile Multi-Stage (`Dockerfile`)
- **Estágio 1 (Restore & Cache de Camadas):**
  - Base: `mcr.microsoft.com/dotnet/sdk:9.0`
  - Copiar `.sln` e arquivos `.csproj` de cada projeto preservando a árvore de pastas.
  - Executar `dotnet restore` isolado para cache otimizado das dependências NuGet.
- **Estágio 2 (Build & Publish):**
  - Copiar todo o código-fonte restante.
  - Executar `dotnet publish src/SmartExchange.Api/SmartExchange.Api.csproj -c Release -o /app/publish --no-restore`.
- **Estágio 3 (Runtime Otimizado):**
  - Base: `mcr.microsoft.com/dotnet/aspnet:9.0-alpine` (ou `mcr.microsoft.com/dotnet/aspnet:9.0`).
  - Definir diretório de trabalho `/app`.
  - Copiar saída do estágio anterior `/app/publish`.
  - Trocar para o usuário não-root nativo do .NET 9: `USER app`.
  - Expor a porta HTTP (padrão .NET 9: `8080`).
  - Ponto de entrada: `ENTRYPOINT ["dotnet", "SmartExchange.Api.dll"]`.

---

## 2. Orquestração Local Completa (`docker-compose.yml`)
- **Serviço `postgres`:**
  - Imagem: `postgres:16-alpine`.
  - Portas expostas: `5432:5432`.
  - Volume persistente: `postgres_data:/var/lib/postgresql/data`.
  - Healthcheck com `pg_isready -U postgres`.
- **Serviço `redis`:**
  - Imagem: `redis:7-alpine`.
  - Portas expostas: `6379:6379`.
  - Healthcheck com `redis-cli ping`.
- **Serviço `api`:**
  - Build contextual apontando para o `Dockerfile` na raiz.
  - Portas mapeadas: `5000:8080`.
  - Dependência com condição:
    - `postgres: condition: service_healthy`
    - `redis: condition: service_healthy`
  - Variáveis de ambiente configurando strings de conexão para a rede interna Docker:
    - `ConnectionStrings__Database=Host=postgres;Port=5432;Database=smartexchange;Username=postgres;Password=postgres;`
    - `ConnectionStrings__Redis=redis:6379`
    - `ASPNETCORE_ENVIRONMENT=Development`

---

## 2.1 Decisões de Implementação e Desvios (registro para ADRs)

- **Testcontainers fixado em 4.0.0 (spec: 4.x).** O Docker Desktop do ambiente de desenvolvimento tem engine 20.10.22, que aceita no máximo a API 1.41. Testcontainers 4.16.0 negocia a API 1.44 (via `Docker.DotNet.Enhanced` 4.3.3), e o engine recusa com `client version 1.44 is too new`. A versão 4.0.0 usa `Docker.DotNet` 3.125.15, compatível com o engine. Alternativa a avaliar em ADR: atualizar o Docker Desktop e voltar para a versão mais recente do Testcontainers.
- **Porta do PostgreSQL no host: 5434** (mesma decisão da Fase 04). O `docker-compose.yml` mantém `5434:5432`, em vez de `5432:5432` da spec, por causa do PostgreSQL nativo na 5432.
- **Credenciais e chave de connection string.** A spec usa `postgres/postgres` e `ConnectionStrings__Database`, mas o contêiner é criado com `smartexchange/smartexchange` e o código lê a chave `AuditDb`. O compose usa `ConnectionStrings__AuditDb` com as credenciais reais do contêiner. Os testes de integração usam o mesmo usuário e banco.
- **Frankfurter substituída por um fake nos testes de integração.** A API real depende de internet e devolve cotações que mudam; por isso `FakeFrankfurterHandler` substitui apenas o `HttpMessageHandler` do `FrankfurterApiClient`. O decorator de cache, o Redis, o PostgreSQL, a resiliência e a serialização continuam reais.
- **FluentAssertions 7.x** (mesma versão das fases anteriores), em vez da 6.x citada na spec, pela mesma razão de licenciamento registrada na Fase 01.
- **Migrations não são aplicadas no startup da API.** A fixture aplica `MigrateAsync` apenas nos testes. No compose, o schema foi criado manualmente na Fase 04 e persiste no volume `postgres-data`. Um ambiente novo sem essa etapa terá a auditoria falhando em silêncio (registrada em log, sem afetar a conversão). Decisão pendente, a registrar em ADR.
- **Dockerfile:** usuário `app` do runtime Alpine. O estágio de restore copia apenas os `.csproj` e o `global.json` antes do código-fonte, para o cache de camadas.
- **CI (`ci.yml`) sem alteração:** o passo `dotnet test SmartExchange.sln` já inclui a suíte de integração, e os runners `ubuntu-latest` têm Docker disponível.

## 3. Projeto de Testes de Integração (`SmartExchange.Tests.Integration`)
- **Novo Projeto xUnit:** `tests/SmartExchange.Tests.Integration/`
- **Pacotes NuGet:**
  - `Microsoft.AspNetCore.Mvc.Testing` (v9.x)
  - `Testcontainers.PostgreSql` (v4.x)
  - `Testcontainers.Redis` (v4.x)
  - `FluentAssertions` (v6.x)
- **Infraestrutura de Teste (`CustomWebApplicationFactory`):**
  - Gerencia o ciclo de vida (`IAsyncLifetime`) de instâncias descartáveis de contêineres PostgreSQL e Redis.
  - Sobrescreve as configurações de injeção de dependência via `ConfigureTestServices`, apontando para os contêineres dinâmicos de teste.
  - Executa a criação automática do banco/tabelas (`EnsureCreatedAsync` ou migrações do EF Core).
- **Cenários de Teste Ponta a Ponta (E2E):**
  1. **Conversão e Auditoria:** Disparar requisição em `GET /api/v1/exchange/convert?from=USD&to=BRL&amount=100&tier=Premium`, validar status 200 e consultar `GET /api/v1/exchange/audit` para verificar a persistência do log de auditoria no PostgreSQL real.
  2. **Cache Efetivo:** Disparar chamada a `GET /api/v1/exchange/currencies`, verificar retorno e inspecionar a existência da chave no contêiner do Redis.