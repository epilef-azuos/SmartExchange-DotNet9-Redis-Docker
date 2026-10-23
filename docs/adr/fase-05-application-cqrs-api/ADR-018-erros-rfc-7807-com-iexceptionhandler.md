# ADR-018: Erros em RFC 7807 com `IExceptionHandler`

- **Status:** Aceito
- **Fase:** 05 - Aplicação (CQRS)

## Contexto

Erros de validação, de domínio e de integração precisam de um formato único e de códigos HTTP semânticos, sem vazar detalhes internos em erros inesperados.

## Decisão

`GlobalExceptionHandler` implementa `IExceptionHandler` (nativo do .NET 9) e escreve a resposta com `IProblemDetailsService`, que produz `application/problem+json` e inclui `traceId`.

| Exceção | Status | Observação |
|---|---|---|
| `DomainException` | 400 | Regra de negócio violada (ex.: moeda com formato inválido) |
| `BadHttpRequestException` | 400 | Corpo JSON malformado ou binding inválido |
| `ValidationException` | 422 | Entrada semanticamente inválida (ex.: montante zero, período fora de 7d/30d/90d) |
| `ExchangeIntegrationException` | 502 | Falha ao consultar o provedor de câmbio |
| Demais | 500 | Detalhe genérico; a exceção completa é registrada em log |

## Consequências

- O cliente recebe sempre o mesmo formato de erro, com `type`, `title`, `status`, `detail` e `traceId`.
- Erros 500 não expõem mensagem interna. O `traceId` permite correlacionar com o log.
- **Ponto em aberto:** o middleware de exceção do ASP.NET registra como erro as exceções de domínio que viram 400, o que polui o log com erros de cliente. É cosmético, mas deve ser ajustado com um filtro de log ou um nível dedicado.
- A tabela de códigos é mantida no próprio handler. Adicionar um novo tipo de exceção exige atualizar o `switch` e os testes.
