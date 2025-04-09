FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

USER $APP_UID

WORKDIR /app

EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

ARG BUILD_CONFIGURATION=Release 

WORKDIR /src

COPY ["Sanki.Api/Sanki.Api.csproj", "Sanki.Api/"]
COPY ["Sanki.Configurations/Sanki.Configurations.csproj", "Sanki.Configurations/"]
COPY ["Sanki.Entities/Sanki.Entities.csproj", "Sanki.Entities/"]
COPY ["Sanki.Persistence/Sanki.Persistence.csproj", "Sanki.Persistence/"]
COPY ["Sanki.Repositories/Sanki.Repositories.csproj", "Sanki.Repositories/"]
COPY ["Sanki.Repositories.Contracts/Sanki.Repositories.Contracts.csproj", "Sanki.Repositories.Contracts/"]
COPY ["Sanki.Services/Sanki.Services.csproj", "Sanki.Services/"]
COPY ["Sanki.Services.Contracts/Sanki.Services.Contracts.csproj", "Sanki.Services.Contracts/"]
COPY ["Sanki.Tests/Sanki.Tests.csproj", "Sanki.Tests/"]

RUN dotnet restore "Sanki.Api/Sanki.Api.csproj"

COPY . .

RUN dotnet build "Sanki.Api/Sanki.Api.csproj" -o ${BUILD_CONFIGURATION} -c /app/build

FROM build AS publish

ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "Sanki.Api/Sanki.Api.csproj" -o ${BUILD_CONFIGURATION} -c /app/publish /p:UseAppHost=false

FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT [ "dotnet", "Sanki.Api.dll" ]