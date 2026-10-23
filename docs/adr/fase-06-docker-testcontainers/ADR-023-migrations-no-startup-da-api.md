# ADR-023: Aplicação de migrations no startup da API

- **Status:** Proposto
- **Fase:** 06 - Docker e Testes de Integração

## Contexto

O schema `conversion_audit_logs` é criado por migration do EF Core. Hoje, a API **não** aplica migrations ao iniciar:

- A fixture de testes aplica `MigrateAsync` explicitamente.
- No compose, o schema existe porque foi criado manualmente na Fase 04 e persiste no volume `postgres-data`.
- Em um ambiente novo (outra máquina, banco recriado, CI com banco persistente), a tabela não existe. A gravação da auditoria falha, o erro é registrado em log, e **as conversões continuam funcionando sem registrar nada**. Essa falha é silenciosa do ponto de vista do cliente da API.

## Opções

1. **`Database.Migrate()` no startup**, sempre. Simples, mas faz a API alterar o banco a cada subida, inclusive em produção, e exige permissão de DDL no usuário da aplicação.
2. **`Database.Migrate()` apenas em `Development`.** Conveniente para desenvolvimento, sem efeito em produção. Não cobre ambientes de homologação.
3. **Job separado de migração** (contêiner de inicialização no compose ou etapa de deploy). A API nunca altera o schema; a migração roda antes dela. É a opção mais segura, com mais configuração.
4. **Manter manual** (`dotnet ef database update`), documentado no README. Sem automação, depende de disciplina.

## Recomendação

Opção 2 para o desenvolvimento local agora, e opção 3 antes de qualquer ambiente compartilhado. Opção 1 não é recomendada porque mistura responsabilidade de DDL com a API.

## Decisão

**Pendente.** Esta ADR deve ser aceita ou substituída antes da primeira implantação em ambiente compartilhado. Até lá, a aplicação da migration é um passo manual documentado no README.

## Consequências

- Enquanto a decisão estiver pendente, um ambiente novo precisa da etapa manual, e a ausência dela não gera erro visível para o cliente da API.
- Recomenda-se incluir um teste ou verificação de sanidade que confirme a existência da tabela na subida, para tornar a falha visível.
