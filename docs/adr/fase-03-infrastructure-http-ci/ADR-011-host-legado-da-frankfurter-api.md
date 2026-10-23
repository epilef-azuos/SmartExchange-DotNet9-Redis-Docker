# ADR-011: Host legado `api.frankfurter.app` da Frankfurter API

- **Status:** Aceito, com risco de depreciação
- **Fase:** 03 - Infraestrutura HTTP

## Contexto

A spec define `https://api.frankfurter.app` como base. Na verificação feita durante a Fase 03, esse host respondia com o formato da API v1, e a documentação atual aponta `api.frankfurter.dev`, com a v2 como referência e a v1 descrita como depreciada, porém ainda disponível.

## Decisão

- Manter o host da spec como padrão em `FrankfurterOptions.BaseUrl`, para não divergir do contrato definido no projeto.
- A URL é configurável por `Frankfurter:BaseUrl`, sem alteração de código, o que permite migrar para `api.frankfurter.dev/v1` ou para a v2 quando necessário.
- Os formatos de resposta usados (`/latest`, `/currencies` e séries temporais) foram confirmados por chamada real durante a fase.

## Consequências

- Risco: se o host legado for desligado, as cotações param até que a configuração seja trocada. Isso deve ser monitorado.
- Migrar para a v2 exige revisar os DTOs (`FrankfurterLatestResponse`, `FrankfurterTimeSeriesResponse`) e os parâmetros de consulta. Isso deve ser feito em uma ADR própria.
- A suíte de testes não depende do host: ver ADR-022.
