# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/PartnerIntegrationBFF.Domain/PartnerIntegrationBFF.Domain.csproj", "src/PartnerIntegrationBFF.Domain/"]
COPY ["src/PartnerIntegrationBFF.Application/PartnerIntegrationBFF.Application.csproj", "src/PartnerIntegrationBFF.Application/"]
COPY ["src/PartnerIntegrationBFF.Infrastructure/PartnerIntegrationBFF.Infrastructure.csproj", "src/PartnerIntegrationBFF.Infrastructure/"]
COPY ["src/PartnerIntegrationBFF.Api/PartnerIntegrationBFF.Api.csproj", "src/PartnerIntegrationBFF.Api/"]
RUN dotnet restore "src/PartnerIntegrationBFF.Api/PartnerIntegrationBFF.Api.csproj"

COPY . .
WORKDIR /src/src/PartnerIntegrationBFF.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "PartnerIntegrationBFF.Api.dll"]