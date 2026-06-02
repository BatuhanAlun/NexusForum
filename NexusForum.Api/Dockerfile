# Multi-stage build keeps the final image ~200MB instead of ~900MB by leaving the SDK behind.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["NexusForum.Api/NexusForum.Api.csproj", "NexusForum.Api/"]
RUN dotnet restore "NexusForum.Api/NexusForum.Api.csproj"

COPY NexusForum.Api/ NexusForum.Api/
WORKDIR "/src/NexusForum.Api"
RUN dotnet publish "NexusForum.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

# Docker CLI so the api can spawn the pentester container (via mounted host docker.sock).
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl ca-certificates \
    && curl -fsSL https://download.docker.com/linux/static/stable/x86_64/docker-27.3.1.tgz \
       | tar -xz -C /usr/local/bin --strip-components=1 docker/docker \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NexusForum.Api.dll"]
