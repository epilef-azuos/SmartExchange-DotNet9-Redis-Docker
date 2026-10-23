# ADR-004: Versões das bibliotecas de teste

- **Status:** Aceito
- **Fase:** 01 - Setup da Solution

## Contexto

A suíte usa FluentAssertions e Moq. Versões mais recentes de ambas mudaram o modelo de distribuição ou de licença, e isso afeta quem pode usar o projeto em contexto comercial.

## Decisão

- **FluentAssertions `7.*`.** A versão 8 passou a exigir licença comercial para uso não gratuito.
- **Moq `4.21.0`**, a versão resolvida na Fase 01. Mantida como referência para os testes de `HttpMessageHandler` (ver ADR-010 da Fase 03).
- O mesmo critério de licença vale para o MediatR, fixado na linha 12.x (ADR-016).

## Consequências

- Atualizar para FluentAssertions 8 ou MediatR 13 exige revisar a licença antes da atualização.
- A spec original citava FluentAssertions 6.x para a suíte de integração; a linha 7.x foi mantida para uniformidade (registrado na Fase 06).
