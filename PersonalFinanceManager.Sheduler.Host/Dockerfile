FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PersonalFinanceManager.Scheduler.Host/PersonalFinanceManager.Scheduler.Host.csproj", "PersonalFinanceManager.Scheduler.Host/"]
COPY ["PersonalFinanceManager.Scheduler/PersonalFinanceManager.Scheduler.csproj", "PersonalFinanceManager.Scheduler/"]
COPY ["PersonalFinanceManager.Shared/PersonalFinanceManager.Shared.csproj", "PersonalFinanceManager.Shared/"]
COPY ["PersonalFinanceManager.Infrastructure/PersonalFinanceManager.Infrastructure.csproj", "PersonalFinanceManager.Infrastructure/"]
RUN dotnet restore "PersonalFinanceManager.Scheduler.Host/PersonalFinanceManager.Scheduler.Host.csproj"
COPY . .
WORKDIR "/src/PersonalFinanceManager.Scheduler.Host"
RUN dotnet build "PersonalFinanceManager.Scheduler.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PersonalFinanceManager.Scheduler.Host.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PersonalFinanceManager.Scheduler.Host.dll"]