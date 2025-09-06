# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copia o csproj e faz restore (camada de cache)
COPY GameOfLifeApi.csproj ./
RUN dotnet restore --no-cache

# Copia o restante do projeto
COPY . .

# (Opcional) limpar caches antigos, às vezes ajuda por trás de proxy
# RUN dotnet nuget locals all --clear

# Publica (deixa o publish fazer restore se precisar)
RUN dotnet publish -c Release -o /app/publish

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app

# HTTP only no container
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

# Copia artefatos publicados
COPY --from=build /app/publish .

# Diretório de dados do LiteDB
RUN mkdir -p /app/App_Data

ENTRYPOINT ["dotnet", "GameOfLifeApi.dll"]
