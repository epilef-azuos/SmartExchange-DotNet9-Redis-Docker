# ADR-012: Pipeline de CI no GitHub Actions

- **Status:** Aceito
- **Fase:** 03 - Infraestrutura HTTP

## Contexto

Mudanças precisam ser verificadas automaticamente (restauração, compilação estrita e testes) antes de chegar à `main`, sem depender da máquina de quem fez a alteração.

## Decisão

Arquivo `.github/workflows/ci.yml`:

- Gatilhos: `push` e `pull_request` para `main` e `feature/**`.
- Passos: checkout, `actions/setup-dotnet@v4` lendo o `global.json`, `dotnet restore`, `dotnet build --no-restore --configuration Release` e `dotnet test --no-build --configuration Release`.

## Consequências

- Todo `pull_request` para `main` roda a suíte, incluindo a de integração adicionada na Fase 06, que exige Docker. Os runners `ubuntu-latest` oferecem Docker.
- Os testes unitários não fazem chamadas de rede (ADR-022 e constitution da fase), então a pipeline não depende da disponibilidade da Frankfurter.
- Validada localmente apenas a sintaxe YAML do arquivo. O resultado das execuções no GitHub Actions não foi verificado neste registro; deve ser conferido na aba Actions do repositório, especialmente para a suíte de integração da Fase 06.
