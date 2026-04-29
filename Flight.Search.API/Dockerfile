FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Flight.Search.API/Flight.Search.API.csproj", "Flight.Search.API/"]
COPY ["Shared.Messages/Shared.Messages.csproj", "Shared.Messages/"]
RUN dotnet restore "Flight.Search.API/Flight.Search.API.csproj"
COPY . .
WORKDIR "/src/Flight.Search.API"
RUN dotnet build "Flight.Search.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Flight.Search.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Flight.Search.API.dll"]
