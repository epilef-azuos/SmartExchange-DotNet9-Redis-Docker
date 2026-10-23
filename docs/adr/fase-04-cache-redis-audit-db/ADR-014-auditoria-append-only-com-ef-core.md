# ADR-014: Auditoria append-only com EF Core e precisão decimal explícita

- **Status:** Aceito
- **Fase:** 04 - Cache e Auditoria

## Contexto

Cada conversão precisa deixar um registro imutável para fins de auditoria. Os valores monetários exigem precisão definida no banco, não apenas no cálculo em memória, para que o que é gravado seja exatamente o que foi calculado.

## Decisão

- `ConversionAuditLog` (Domain) não tem setters públicos. É criado por `FromConversion(id, result, createdAtUtc)`, com construtor privado para o EF Core.
- `IConversionAuditRepository` (Application) expõe apenas `AddAsync` e `GetRecentAuditsAsync`. Não existe atualização nem remoção.
- Tabela `conversion_audit_logs` com precisão explícita:
  - `numeric(18,2)` para quantias: `BaseAmount`, `ConvertedAmount`, `SpreadAmount`
  - `numeric(18,6)` para taxas e percentuais: `BaseRate`, `FinalRateWithSpread`, `SpreadPercentage`
- `CreatedAtUtc` em `timestamp with time zone`, com índice para consultas de histórico.
- Tipos de cliente (`ClientTier`, moedas) gravados como texto.

## Consequências

- A imutabilidade é garantida pela API do repositório, não pelo banco. Uma alteração feita diretamente no SQL não é impedida; isso fica como risco aceito.
- A precisão está testada sobre o modelo do EF (`AuditDbContextModelTests`), sem conexão com banco.
- A migration inicial foi gerada e aplicada no PostgreSQL local; o caminho de criação de schema em outros ambientes é tratado em ADR-023.
