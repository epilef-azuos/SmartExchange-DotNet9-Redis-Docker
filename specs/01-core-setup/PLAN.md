# PLAN: Fase 01

1. **Setup da Solution (CLI)**
   - Criar arquivo `global.json` limitando a versão ao .NET 9.
   - Criar os 4 projetos da Clean Architecture e 1 projeto de Testes Unitários.
   - Adicionar referências de projetos (`Api` -> `Application` e `Infrastructure`, `Application` -> `Domain`, `Infrastructure` -> `Application`, etc.).
   - Criar o `docker-compose.yml` na raiz.
2. **Criação dos Value Objects**
   - Criar a struct ou record `Currency` (validando o código ISO de 3 letras).
   - Criar o record `Money` (agrupando `decimal Amount` e `Currency`).
3. **Implementação do Strategy Pattern (Spread)**
   - Criar a interface `ISpreadStrategy`.
   - Implementar `Standard`, `Premium` e `Vip`.
   - Testar unitariamente cada estratégia usando `xUnit` e `FluentAssertions`.