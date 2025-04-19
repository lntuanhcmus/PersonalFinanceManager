FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 7204

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PersonalFinanceManager.WebHost/PersonalFinanceManager.WebHost.csproj", "PersonalFinanceManager.WebHost/"]
COPY ["PersonalFinanceManager.Shared/PersonalFinanceManager.Shared.csproj", "PersonalFinanceManager.Shared/"]
COPY ["PersonalFinanceManager.Infrastructure/PersonalFinanceManager.Infrastructure.csproj", "PersonalFinanceManager.Infrastructure/"]
RUN dotnet restore "PersonalFinanceManager.WebHost/PersonalFinanceManager.WebHost.csproj"
COPY . .
WORKDIR "/src/PersonalFinanceManager.WebHost"
RUN dotnet build "PersonalFinanceManager.WebHost.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PersonalFinanceManager.WebHost.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PersonalFinanceManager.WebHost.dll"]