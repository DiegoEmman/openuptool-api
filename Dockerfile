# Dockerfile para producción (opcional)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/OpenUpTool.Api/OpenUpTool.Api.csproj", "OpenUpTool.Api/"]
COPY ["src/OpenUpTool.Core/OpenUpTool.Core.csproj", "OpenUpTool.Core/"]
COPY ["src/OpenUpTool.Infrastructure/OpenUpTool.Infrastructure.csproj", "OpenUpTool.Infrastructure/"]
RUN dotnet restore "OpenUpTool.Api/OpenUpTool.Api.csproj"

COPY src/ .
WORKDIR "/src/OpenUpTool.Api"
RUN dotnet build "OpenUpTool.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OpenUpTool.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OpenUpTool.Api.dll"]
