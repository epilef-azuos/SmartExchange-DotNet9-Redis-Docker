# TASK: Docker Multi-Stage, Orquestração e Testcontainers

Status Geral: CONCLUÍDA (build, testes unitários e de integração, e compose com healthchecks validados; commit pendente)
Fase: 06 - Docker Multi-Stage & Testcontainers
Branch: feature/06-docker-testcontainers

---

## 1. Controle de Versão e Setup de Testes de Integração
- [x] Criar e chavear para a branch `feature/06-docker-testcontainers`.
- [x] Criar projeto `tests/SmartExchange.Tests.Integration` (`dotnet new xunit`).
- [x] Adicionar o projeto de testes à `SmartExchange.sln`.
- [x] Adicionar referência de `SmartExchange.Api` ao projeto de testes de integração.
- [x] Instalar pacotes em `SmartExchange.Tests.Integration`:
  - `Microsoft.AspNetCore.Mvc.Testing`
  - `Testcontainers.PostgreSql`
  - `Testcontainers.Redis`
  - `FluentAssertions`

## 2. Infraestrutura de Testcontainers e WebApplicationFactory
- [x] Implementar classe `SmartExchangeApiFactory.cs` herdando de `WebApplicationFactory<Program>` e implementando `IAsyncLifetime`.
- [x] Configurar inicialização dinâmica de contêineres `PostgreSqlBuilder` e `RedisBuilder`.
- [x] Sobrescrever configuração de connection strings no `ConfigureAppConfiguration` ou `ConfigureWebHost`.
- [x] Garantir migração/criação automática do schema de banco no startup de testes.

## 3. Testes de Integração Ponta a Ponta
- [x] Implementar `ExchangeEndpointsIntegrationTests.cs`:
  - Validar chamada em `GET /api/v1/exchange/convert` retornando status 200.
  - Validar que a transação foi salva e pode ser lida em `GET /api/v1/exchange/audit`.
  - Validar endpoint `POST /api/v1/exchange/portfolio/valuate` com cálculo de proporção.
  - Validar cache no Redis para `GET /api/v1/exchange/currencies`.

## 4. Dockerfile Multi-Stage e Docker Compose
- [x] Criar `Dockerfile` multi-stage na raiz do projeto com usuário não-root `USER app` e base `.NET 9`.
- [x] Atualizar `docker-compose.yml` na raiz:
  - Adicionar o serviço `api` apontando para o build do Dockerfile.
  - Configurar healthchecks no `postgres` e `redis`.
  - Definir `depends_on` com `condition: service_healthy`.
  - Mapear variáveis de ambiente de banco e cache.

## 5. Validação, Compilação e Execução
- [x] Executar `dotnet build` sem erros ou warnings.
- [x] Executar `dotnet test` garantindo que tanto testes unitários quanto de integração passem com 100% de sucesso.
- [x] Testar subida local via `docker compose up --build -d` e verificar status `healthy` dos contêineres.
- [x] Marcar todos os checkboxes deste `TASK.md` como `[x]`.