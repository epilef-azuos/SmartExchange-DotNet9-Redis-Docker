# ADR-024: Alinhamento de credenciais e chaves de connection string no compose

- **Status:** Aceito
- **Fase:** 06 - Docker e Testes de Integração

## Contexto

A spec da Fase 06 define para a API no compose:

- `ConnectionStrings__Database` com `Username=postgres;Password=postgres`
- `ConnectionStrings__Redis=redis:6379`

Mas o código lê a chave `AuditDb` (definida na Fase 04), e o contêiner do PostgreSQL é criado com `POSTGRES_USER=smartexchange` e `POSTGRES_PASSWORD=smartexchange`, criados na Fase 01. Com as chaves da spec, a API não encontraria a connection string de auditoria, e com as credenciais da spec a autenticação falharia.

## Decisão

No `docker-compose.yml`:

- A chave é `ConnectionStrings__AuditDb`, a mesma lida pelo código.
- As credenciais são `smartexchange/smartexchange`, as do contêiner. Nomes de banco, usuário e senha seguem o que já existe no volume persistente.
- `ConnectionStrings__Redis=redis:6379` foi mantido, pois já coincide com o código.

Os testes de integração usam as mesmas credenciais (`smartexchange`), pela mesma razão.

## Alternativas consideradas

- Alterar o código para ler `Database` em vez de `AuditDb`: mudaria o contrato já registrado nas fases 04 e 05 sem ganho real.
- Recriar o volume com as credenciais da spec: destrói o banco local por conveniência. Rejeitado.

## Consequências

- O desvio em relação à spec está documentado aqui e no `SPEC.md` da Fase 06 (seção 2.1).
- As credenciais são de desenvolvimento e estão em texto claro no compose e no `appsettings.json`. Não devem ser reutilizadas em nenhum ambiente real; isso deve ser tratado com segredos gerenciados em uma ADR de segurança futura.
