# PLAN: Fase 03

1. **Branch e Preparação de Pacotes**
   - Criar e chavear para a branch `feature/03-infrastructure-http-ci` a partir da `main` atualizada.
   - Adicionar o pacote `Microsoft.Extensions.Http.Resilience` ao projeto `SmartExchange.Infrastructure`.
2. **Definição dos Contratos e DTOs na Application**
   - Criar interface `IExchangeRateProvider` em `SmartExchange.Application/Common/Interfaces/`.
   - Criar exceções de integração da aplicação em `SmartExchange.Application/Common/Exceptions/`.
3. **Implementação do Cliente HTTP e Resiliência na Infrastructure**
   - Criar DTOs de deserialização da Frankfurter API em `SmartExchange.Infrastructure/Http/Dtos/`.
   - Implementar `FrankfurterApiClient` em `SmartExchange.Infrastructure/Http/Clients/`.
   - Criar extensão de DI `AddInfrastructureHttpServices` configurando o `HttpClient` tipado e o pipeline de resiliência nativo.
4. **Testes Unitários de Resiliência e Deserialização**
   - Criar `FrankfurterApiClientTests.cs` em `tests/SmartExchange.Tests.Unit/Infrastructure/Http/`.
   - Validar cenários de sucesso, erro de API e comportamento com `HttpMessageHandler` mockado.
5. **Configuração da Pipeline GitHub Actions**
   - Criar o diretório `.github/workflows/` e o arquivo `ci.yml`.
6. **Validação e Build Final**
   - Executar `dotnet build` e `dotnet test`.
   - Atualizar `TASK.md` com os checks correspondentes.