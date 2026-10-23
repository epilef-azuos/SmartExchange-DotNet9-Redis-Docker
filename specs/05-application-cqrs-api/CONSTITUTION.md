# Constitution: Fase 05 - Application Layer (CQRS), Endpoints BFF e Auditoria Assíncrona

1. **Segregação Estrita CQRS:** Comandos que geram efeitos colaterais e logs de auditoria (`ConvertCurrencyCommand`) devem ser tratados como Commands; consultas a cotações, portfólio, séries temporais e logs são Queries.
2. **Desacoplamento de Auditoria via MediatR Notifications:** A persistência da auditoria não deve introduzir latência desnecessária no retorno da conversão cambial. O handler do comando deve publicar uma notificação (`ConversionExecutedNotification`), tratada por um `INotificationHandler` dedicado responsável por persistir o registro no `IConversionAuditRepository`.
3. **Design Otimizado para BFF (Frontend-Ready):** As respostas das APIs devem fornecer dados prontos para o consumo da interface, eliminando processamento pesado no cliente:
   - A valoração de carteira deve entregar totais consolidados e a fatia percentual (`SharePercentage`) calculada para gráficos de pizza.
   - O histórico cambial deve ser formatado como uma lista ordenada de pontos temporais (`Date`, `Rate`) para gráficos de linha.
4. **Tratamento Global de Erros e Tipagem Consistente:** Erros de validação (ex: moedas inválidas) e exceções de domínio/integração devem retornar payloads padronizados no formato Problem Details (RFC 7807), garantindo códigos HTTP semanticamente corretos (400, 422, 502, 500).