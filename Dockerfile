# syntax=docker/dockerfile:1

# Estágio 1: restore isolado, para que o cache de camadas sobreviva a mudanças de código-fonte.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS restore
WORKDIR /src
COPY global.json SmartExchange.sln ./
COPY src/SmartExchange.Api/SmartExchange.Api.csproj src/SmartExchange.Api/
COPY src/SmartExchange.Application/SmartExchange.Application.csproj src/SmartExchange.Application/
COPY src/SmartExchange.Domain/SmartExchange.Domain.csproj src/SmartExchange.Domain/
COPY src/SmartExchange.Infrastructure/SmartExchange.Infrastructure.csproj src/SmartExchange.Infrastructure/
COPY tests/SmartExchange.Tests.Unit/SmartExchange.Tests.Unit.csproj tests/SmartExchange.Tests.Unit/
COPY tests/SmartExchange.Tests.Integration/SmartExchange.Tests.Integration.csproj tests/SmartExchange.Tests.Integration/
RUN dotnet restore SmartExchange.sln

# Estágio 2: código-fonte completo e publicação da API.
FROM restore AS build
COPY . .
RUN dotnet publish src/SmartExchange.Api/SmartExchange.Api.csproj -c Release -o /app/publish --no-restore

# Estágio 3: runtime mínimo, sem SDK e sem privilégios administrativos.
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app
COPY --from=build /app/publish .
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "SmartExchange.Api.dll"]
