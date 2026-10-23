# ADR-021: Testcontainers 4.0.0 por compatibilidade com a API do Docker

- **Status:** Aceito
- **Fase:** 06 - Docker e Testes de Integração

## Contexto

A suíte de integração sobe PostgreSQL e Redis reais com Testcontainers. Na primeira execução, com `Testcontainers 4.16.0` (versão mais recente aceita pela spec, `4.x`), todos os testes falharam antes de iniciar qualquer contêiner:

```
Docker API responded with status code='BadRequest',
response='{"message":"client version 1.44 is too new. Maximum supported API version is 1.41"}'
```

O ambiente tem Docker Desktop 4.16.3 com engine `20.10.22`, cuja API máxima é a `1.41`.

## Causa

- Testcontainers 4.16.0 depende de `Docker.DotNet.Enhanced` 4.3.3, que negocia a API `1.44` por padrão.
- A biblioteca não lê nenhuma variável de ambiente para fixar a versão da API. Verificado nas DLLs do pacote.

## Decisão

Fixar `Testcontainers.PostgreSql` e `Testcontainers.Redis` em `4.0.0`. Essa versão depende de `Docker.DotNet` 3.125.15, cuja negociação de API é compatível com o engine instalado. Confirmado com `dotnet test`: os cinco testes passaram e os contêineres foram removidos ao final.

## Alternativas consideradas

- **Atualizar o Docker Desktop** para uma versão com engine mais novo. É a solução definitiva, mas altera o ambiente de todas as pessoas do projeto e depende de decisão externa à aplicação. Registrada como alternativa a avaliar.
- **Variável de ambiente para a versão da API:** não é suportada pela biblioteca.

## Consequências

- A versão 4.0.0 é mais antiga que a 4.16.0 e pode não ter correções posteriores. Ao atualizar o Docker Desktop, esta ADR deve ser revisitada.
- Engines mais novos costumam aceitar versões de API anteriores à máxima, então a versão fixada deve funcionar também no runner `ubuntu-latest`. Isso não foi verificado no GitHub Actions e deve ser conferido na primeira execução da pipeline.
- A limitação é de ambiente e não do código; por isso ficou registrada aqui, e não como débito técnico do projeto.
