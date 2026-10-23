# TASK: Setup Inicial, Clean Architecture e Infraestrutura Docker

Status Geral: EM PROGRESSO
Fase: 01 - Core Setup

---

## 1. Scaffolding da Solution e Projetos .NET 9
- [x] Criar arquivo `global.json` travando o SDK na versão `9.0.xxx`.
- [x] Criar a Solution raiz `SmartExchange.sln`.
- [x] Criar o projeto Web API em `src/SmartExchange.Api` (`dotnet new webapi`).
- [x] Criar a Class Library em `src/SmartExchange.Application` (`dotnet new classlib`).
- [x] Criar a Class Library em `src/SmartExchange.Domain` (`dotnet new classlib`).
- [x] Criar a Class Library em `src/SmartExchange.Infrastructure` (`dotnet new classlib`).
- [x] Criar o projeto de Testes em `tests/SmartExchange.Tests.Unit` (`dotnet new xunit`).
- [x] Adicionar todos os projetos à `SmartExchange.sln`.

## 2. Configuração de Dependências entre Projetos (Clean Architecture)
- [x] Vincular referências em `SmartExchange.Application` -> `SmartExchange.Domain`.
- [x] Vincular referências em `SmartExchange.Infrastructure` -> `SmartExchange.Application`.
- [x] Vincular referências em `SmartExchange.Api` -> `SmartExchange.Application` e `SmartExchange.Infrastructure`.
- [x] Vincular referências em `SmartExchange.Tests.Unit` -> `SmartExchange.Domain` e `SmartExchange.Application`.
- [x] Instalar pacotes de teste no projeto unitário: `FluentAssertions` e `Moq`.

## 3. Infraestrutura Local (Docker)
- [x] Criar `.dockerignore` na raiz.
- [x] Criar `docker-compose.yml` na raiz com os serviços:
  - `postgres` (imagem `postgres:16-alpine`, porta `5434:5432`, volume mapeado, credenciais padrão).
  - Ajuste pós-fase: a porta do host era `5432:5432` na entrega original; foi alterada para `5434` na Fase 04, por conflito com o PostgreSQL 15 nativo do Windows. Ver `specs/04-cache-redis-audit-db/SPEC.md`, seção 4.
  - `redis` (imagem `redis:7-alpine`, porta `6379:6379`).

## 4. Validação e Baseline de Compilação
- [x] Limpar arquivos padrão gerados pelos templates (`Class1.cs`, controllers de exemplo).
- [x] Executar `dotnet restore`.
- [x] Executar `dotnet build` garantindo saída com 0 erros e 0 warnings.
- [x] Executar `dotnet test` garantindo execução limpa da suíte.