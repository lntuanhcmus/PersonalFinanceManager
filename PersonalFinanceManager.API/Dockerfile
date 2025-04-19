FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PersonalFinanceManager.API/PersonalFinanceManager.API.csproj", "PersonalFinanceManager.API/"]
COPY ["PersonalFinanceManager.Shared/PersonalFinanceManager.Shared.csproj", "PersonalFinanceManager.Shared/"]
COPY ["PersonalFinanceManager.Infrastructure/PersonalFinanceManager.Infrastructure.csproj", "PersonalFinanceManager.Infrastructure/"]
RUN dotnet restore "PersonalFinanceManager.API/PersonalFinanceManager.API.csproj"
COPY . .
WORKDIR "/src/PersonalFinanceManager.API"
RUN dotnet build "PersonalFinanceManager.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PersonalFinanceManager.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PersonalFinanceManager.API.dll"]