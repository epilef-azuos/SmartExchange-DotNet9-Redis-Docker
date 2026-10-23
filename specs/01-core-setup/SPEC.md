# SPEC: Setup Inicial e Lógica de Spread

## Objetivo
Estabelecer a fundação do SmartExchange Gateway utilizando .NET 9 e modelar as regras de negócios puras (cálculo de conversão com Spread).

## Requisitos Técnicos
- **SDK:** .NET 9.0.
- **Projetos:** 
  - `SmartExchange.Api` (Web API)
  - `SmartExchange.Application` (Class Library)
  - `SmartExchange.Domain` (Class Library)
  - `SmartExchange.Infrastructure` (Class Library)
  - `SmartExchange.Tests.Unit` (xUnit)
- **Infraestrutura Local:** Arquivo `docker-compose.yml` na raiz contendo serviços para `postgres:16-alpine` e `redis:7-alpine`.
  - Ajuste pós-fase: o PostgreSQL é publicado no host na porta `5434` (originalmente `5432`), por conflito com o PostgreSQL 15 nativo do Windows. Ver `specs/04-cache-redis-audit-db/SPEC.md`, seção 4.
- **Domain Logic:**
  - Interface `ISpreadStrategy` com método `decimal CalculateSpread(decimal baseAmount)`.
  - Implementações: `StandardSpreadStrategy` (+3.5%), `PremiumSpreadStrategy` (+1.5%), `VipSpreadStrategy` (+0.5%).
  - Factory ou Enum mapping para resolver a estratégia correta com base na entrada do usuário.