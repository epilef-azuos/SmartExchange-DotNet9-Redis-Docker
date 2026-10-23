# ADR-015: PostgreSQL do Docker publicado na porta 5434 do host

- **Status:** Aceito
- **Fase:** 04 - Cache e Auditoria (aplicada também nas Fases 01 e 06)

## Contexto

A spec define `5432:5432` para o PostgreSQL do compose. No ambiente de desenvolvimento há uma instalação nativa do PostgreSQL 15 do Windows escutando em `0.0.0.0:5432` (serviço `postgresql-x64-15`), além do PostgreSQL 18 na porta 5433.

Com o mapeamento `5432:5432`, `localhost:5432` era atendido pelo PostgreSQL 15 nativo, que não tem a role `smartexchange`. A aplicação falhava com `28000: role "smartexchange" does not exist`, mesmo com o container saudável e com a role criada.

## Decisão

- Publicar o container no host como `"5434:5432"`. A porta interna continua 5432.
- Atualizar a connection string `AuditDb` para `Port=5434`.
- Não parar nem alterar o PostgreSQL nativo, que pertence a outros projetos.

## Alternativas consideradas

- Parar o serviço nativo (`Stop-Service postgresql-x64-15`): afeta outros projetos da máquina.
- Criar a role e o banco na instalação nativa: mistura o ambiente do projeto com uma instalação genérica.

## Consequências

- A spec original (`5432:5432`) e a Fase 01 foram atualizadas com nota de ajuste, sem reescrever o registro histórico.
- Em CI e em outros ambientes, a porta do host é irrelevante, porque a comunicação entre os contêineres acontece pela rede do compose (`postgres:5432`).
- Quem clonar o repositório e tiver o PostgreSQL nativo na 5432 não precisa de nenhuma ação; quem não tiver também não. A mudança só importa para conexões do host ao container.
