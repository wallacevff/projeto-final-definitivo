FROM node:20-alpine AS frontend-build
WORKDIR /app

COPY src/ProjetoFinal.ClientApp/package*.json ./
RUN npm ci

COPY src/ProjetoFinal.ClientApp/ ./
RUN npm run ng -- build --configuration=production --delete-output-path --output-path dist --progress --aot --output-hashing all

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ProjetoFinal.sln ./
COPY Directory.Build.props ./
COPY src/ProjetoFinal.Domain.Shared/ProjetoFinal.Domain.Shared.csproj src/ProjetoFinal.Domain.Shared/
COPY src/ProjetoFinal.Domain/ProjetoFinal.Domain.csproj src/ProjetoFinal.Domain/
COPY src/ProjetoFinal.Application.Contracts/ProjetoFinal.Application.Contracts.csproj src/ProjetoFinal.Application.Contracts/
COPY src/ProjetoFinal.Aplication.Services/ProjetoFinal.Aplication.Services.csproj src/ProjetoFinal.Aplication.Services/
COPY src/ProjetoFinal.Infra.CrossCutting/ProjetoFinal.Infra.CrossCutting.csproj src/ProjetoFinal.Infra.CrossCutting/
COPY src/ProjetoFinal.Infra.Data/ProjetoFinal.Infra.Data.csproj src/ProjetoFinal.Infra.Data/
COPY src/ProjetoFinal.IoC/ProjetoFinal.IoC.csproj src/ProjetoFinal.IoC/
COPY src/ProjetoFinal.Api/ProjetoFinal.Api.csproj src/ProjetoFinal.Api/
COPY src/ProjetoFinal.ClientApp/ProjetoFinal.ClientApp.esproj src/ProjetoFinal.ClientApp/

RUN dotnet restore ProjetoFinal.sln

COPY src/ ./src/

RUN dotnet publish src/ProjetoFinal.Api/ProjetoFinal.Api.csproj \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=backend-build /app/publish ./
COPY --from=frontend-build /app/dist/browser/ ./wwwroot/

ENTRYPOINT ["dotnet", "ProjetoFinal.Api.dll"]
