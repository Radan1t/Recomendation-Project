# Використовуємо офіційний образ ASP.NET Core для запуску
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Використовуємо SDK для збірки (компіляції)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копіюємо файли проектів (csproj) для відновлення залежностей
# Зверніть увагу: шляхи відносні до кореня папки 'backend'
COPY ["ApiGateway/ApiGateway.csproj", "ApiGateway/"]
COPY ["Shared/Shared.csproj", "Shared/"]

# Відновлюємо пакети (dotnet restore)
RUN dotnet restore "ApiGateway/ApiGateway.csproj"

# Копіюємо весь інший вихідний код
COPY . .

# Збираємо проект
WORKDIR "/src/ApiGateway"
RUN dotnet build "ApiGateway.csproj" -c Release -o /app/build

# Публікуємо (створюємо фінальні dll)
FROM build AS publish
RUN dotnet publish "ApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Фінальний етап - копіюємо опубліковані файли у образ для запуску
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApiGateway.dll"]