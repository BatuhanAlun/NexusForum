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

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NexusForum.Api.dll"]
