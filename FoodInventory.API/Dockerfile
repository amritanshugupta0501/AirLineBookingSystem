FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FoodInventory.API/FoodInventory.API.csproj", "FoodInventory.API/"]
RUN dotnet restore "FoodInventory.API/FoodInventory.API.csproj"
COPY . .
WORKDIR "/src/FoodInventory.API"
RUN dotnet build "FoodInventory.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FoodInventory.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FoodInventory.API.dll"]
