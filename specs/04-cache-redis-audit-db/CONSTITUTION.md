# Constitution: Fase 04 - Cache Distribuído (Decorator) e Auditoria Imutável (EF Core)

1. **Princípio da Responsabilidade Única (SRP) & Decorator Pattern:** A lógica de cache não deve poluir a implementação do cliente HTTP (`FrankfurterApiClient`). O cache deve ser estritamente injetado através de um `CachedExchangeRateProvider` que implementa `IExchangeRateProvider` e decora a instância real usando DI nativa do .NET 9.
2. **Políticas de TTL Especializadas:** O cache no Redis deve respeitar as características de volatilidade de cada dado:
   - Cotações em tempo real (`latest`): TTL curto (15 minutos).
   - Séries históricas (`history`): TTL longo (24 horas), visto que dados consolidados do passado são imutáveis.
   - Dicionário de moedas (`currencies`): TTL estendido (7 dias).
3. **Resiliência a Falhas de Cache (Cache-Aside Transparente):** Falhas transitórias no Redis (ex: indisponibilidade do nó) não devem quebrar o fluxo de câmbio; o decorator deve capturar exceções de cache, logar o alerta e degradar graciosamente consultando o provedor HTTP interno.
4. **Imutabilidade e Isolamento de Auditoria:** O log de auditoria é append-only (somente escrita e leitura histórica). Não deve haver métodos de update ou delete para a entidade de transação no EF Core. O `AuditDbContext` e entidades de persistência devem mapear tipos com exatidão (`decimal(18,6)` para taxas e `decimal(18,2)` para quantias monetárias).