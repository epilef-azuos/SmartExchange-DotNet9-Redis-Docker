# ADR-003: Build output fora do versionamento

- **Status:** Aceito
- **Fase:** 01 - Setup da Solution

## Contexto

O commit inicial da Fase 01 (`f18b07b`) foi feito sem `.gitignore`, e por isso versionou as pastas `bin/` e `obj/` de todos os projetos, com cerca de 245 binários e artefatos gerados. Isso polui o histórico, gera diffs a cada build e pode expor artefatos de ambiente local.

## Decisão

- Gerar o `.gitignore` padrão do .NET (`dotnet new ignore`), que cobre `[Bb]in/` e `[Oo]bj/`.
- Remover os artefatos do índice com `git rm --cached` (os arquivos continuam no disco), em commit próprio (`3363c93`).
- **Não reescrever o histórico.** Os binários permanecem nos commits antigos. Remover de forma definitiva exigiria `git filter-repo` e force-push, o que não foi autorizado.

## Consequências

- Novos builds não geram diffs versionados.
- Os binários antigos continuam acessíveis no histórico do repositório remoto. Se isso for um problema (tamanho, dados sensíveis), a remoção definitiva é uma decisão separada, a ser registrada em outra ADR.
- A lição vale para o template: o `.gitignore` deve ser criado antes do primeiro build versionado.
