FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ContentService/ContentService.csproj", "ContentService/"]
COPY ["Shared/Shared.csproj", "Shared/"]

RUN dotnet restore "ContentService/ContentService.csproj"

COPY . .
WORKDIR "/src/ContentService"
RUN dotnet build "ContentService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ContentService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ContentService.dll"]