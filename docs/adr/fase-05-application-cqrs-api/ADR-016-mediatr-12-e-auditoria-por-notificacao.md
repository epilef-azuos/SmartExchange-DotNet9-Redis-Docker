# ADR-016: MediatR 12 e auditoria desacoplada por notificação

- **Status:** Aceito
- **Fase:** 05 - Aplicação (CQRS)

## Contexto

A constitution da Fase 05 exige separar comandos de consultas e tirar a auditoria do fluxo principal da conversão. Escrever o registro de auditoria diretamente no handler do comando acoplaria os dois assuntos.

## Decisão

- **MediatR fixado em `12.*`.** As versões 13 e superiores exigem licença comercial para uso acima dos limites gratuitos. A 12.x é a última distribuída sob Apache-2.0.
- O `ConvertCurrencyCommandHandler` publica `ConversionExecutedNotification` via `IPublisher` após calcular a conversão.
- `ConversionAuditEventHandler` (`INotificationHandler`) cria o `ConversionAuditLog` e grava pelo `IConversionAuditRepository`.
- Falhas na persistência são capturadas e registradas com `LogError`. A conversão já calculada não é invalidada.

## Consequências

- **Latência:** o `IPublisher` do MediatR 12 aguarda os handlers. A gravação da auditoria, portanto, soma latência à resposta da conversão. O desacoplamento é de código, não de tempo de execução.
- **Perda de auditoria:** uma falha de banco faz o registro se perder silenciosamente, exceto pelo log. Isso é um trade-off aceito, porque a conversão não deve falhar por causa da auditoria.
- **Alternativa futura:** uma fila em memória com `Channel<T>` e um `BackgroundService` removeria a latência. Exige decidir sobre perda em caso de desligamento, e deve ser registrada em ADR própria.
- **Fora de escopo:** a lógica do handler de auditoria não é coberta em teste de integração contra o banco real; o teste unitário verifica a chamada ao repositório e o comportamento em falha.
