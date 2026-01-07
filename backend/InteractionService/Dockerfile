FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["InteractionService/InteractionService.csproj", "InteractionService/"]
COPY ["Shared/Shared.csproj", "Shared/"]

RUN dotnet restore "InteractionService/InteractionService.csproj"

COPY . .
WORKDIR "/src/InteractionService"
RUN dotnet build "InteractionService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InteractionService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InteractionService.dll"]