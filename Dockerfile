FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY StockMonitorSolution.sln .
COPY StockMonitor/StockMonitor.csproj ./StockMonitor/
COPY StockMonitor.Tests/StockMonitor.Tests.csproj ./StockMonitor.Tests/

RUN dotnet restore StockMonitorSolution.sln

COPY . .

RUN dotnet publish "StockMonitor/StockMonitor.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final

WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "StockMonitor.dll"]