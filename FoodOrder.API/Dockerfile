FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FoodOrder.API/FoodOrder.API.csproj", "FoodOrder.API/"]
RUN dotnet restore "FoodOrder.API/FoodOrder.API.csproj"
COPY . .
WORKDIR "/src/FoodOrder.API"
RUN dotnet build "FoodOrder.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FoodOrder.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FoodOrder.API.dll"]
