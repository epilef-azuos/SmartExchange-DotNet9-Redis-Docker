# PLAN: Fase 06

1. **Branch e Projeto de Testes de Integração**
   - Criar e chavear para a branch `feature/06-docker-testcontainers`.
   - Criar o projeto `tests/SmartExchange.Tests.Integration` via .NET CLI (`dotnet new xunit`).
   - Adicionar o projeto de testes à solution `SmartExchange.sln`.
   - Adicionar as referências do projeto: `SmartExchange.Api` e `SmartExchange.Infrastructure`.
   - Instalar pacotes do Testcontainers e `Microsoft.AspNetCore.Mvc.Testing`.
2. **Implementação da Fixture com Testcontainers**
   - Implementar `SmartExchangeApiFactory.cs` herdando de `WebApplicationFactory<Program>` e implementando `IAsyncLifetime`.
   - Configurar inicialização e parada limpa dos contêineres PostgreSql e Redis.
   - Configurar injeção dinâmica de ConnectionStrings.
3. **Escrita dos Testes de Integração Ponta a Ponta**
   - Implementar `ExchangeConversionIntegrationTests.cs` (fluxo de conversão e verificação em auditoria).
   - Implementar `CurrenciesCacheIntegrationTests.cs` (validação de gravação física no Redis).
4. **Construção do Dockerfile e Atualização do Docker Compose**
   - Criar `Dockerfile` multi-stage na raiz do repositório com usuário `app`.
   - Atualizar `docker-compose.yml` para orquestrar API, Postgres e Redis com healthchecks vinculados.
5. **Atualização da Pipeline CI (GitHub Actions)**
   - Garantir que `.github/workflows/ci.yml` execute também a suíte de testes de integração com suporte a contêineres Docker.
6. **Validação Final e Fechamento**
   - Executar suíte completa (`dotnet test`).
   - Realizar `docker compose build` e validar inicialização dos 3 contêineres.
   - Atualizar checks em `TASK.md`.