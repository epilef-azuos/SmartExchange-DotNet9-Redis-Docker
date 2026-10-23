# ADR-020: Dockerfile multi-stage com usuário não-root

- **Status:** Aceito
- **Fase:** 06 - Docker e Testes de Integração

## Contexto

A imagem da API não deve conter SDK, código-fonte nem ferramentas de build, e deve rodar sem privilégios administrativos. Além disso, rebuilds frequentes devem reaproveitar o restore de pacotes NuGet quando só o código muda.

## Decisão

Três estágios:

1. **`restore`** (`sdk:9.0`): copia `global.json`, `SmartExchange.sln` e apenas os `.csproj` de cada projeto, preservando a árvore de pastas, e executa `dotnet restore`. Essa camada só é invalidada quando um projeto ou dependência muda.
2. **`build`**: copia o restante do código e executa `dotnet publish -c Release --no-restore`.
3. **`final`** (`aspnet:9.0-alpine`): copia apenas a saída publicada, define `USER app` (usuário padrão das imagens do .NET 9), expõe a porta 8080 e usa `ENTRYPOINT ["dotnet", "SmartExchange.Api.dll"]`.

## Consequências

- A imagem final não contém SDK, código-fonte, `obj/` nem `bin/` de build.
- Mudanças só de código não refazem o restore.
- O `.dockerignore` exclui `bin/`, `obj/`, `.git`, `specs/` e `*.md`, para não enviar contexto desnecessário ao daemon.
- A imagem `sdk:9.0` precisa oferecer um patch compatível com o `global.json` (9.0.318 ou superior, ver ADR-001 da Fase 01).
- Em Windows, o build de imagens Linux depende do Docker Desktop no modo Linux.
