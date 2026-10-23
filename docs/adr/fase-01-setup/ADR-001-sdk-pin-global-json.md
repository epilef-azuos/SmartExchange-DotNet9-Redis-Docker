# ADR-001: Fixar o SDK do .NET 9 com `global.json`

- **Status:** Aceito
- **Fase:** 01 - Setup da Solution

## Contexto

O projeto precisa compilar de forma idêntica na máquina do desenvolvedor, no CI e na imagem Docker. Sem fixação, cada ambiente usa o SDK instalado, o que pode gerar diferenças de comportamento de build.

## Decisão

Arquivo `global.json` na raiz com `"version": "9.0.318"` e `"rollForward": "latestPatch"`. O SDK pode subir de patch dentro da faixa 9.0, mas não muda de feature band.

## Consequências

- Builds reproduzíveis entre desenvolvedor, CI (`actions/setup-dotnet` lê o `global.json`) e Docker (`sdk:9.0`).
- Atualizar o SDK exige alterar o `global.json` explicitamente.
- O runner do CI precisa oferecer o patch 9.0.318 ou superior; caso contrário, o setup falha de forma explícita.
