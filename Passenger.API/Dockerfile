FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Passenger.API/Passenger.API.csproj", "Passenger.API/"]
RUN dotnet restore "Passenger.API/Passenger.API.csproj"
COPY . .
WORKDIR "/src/Passenger.API"
RUN dotnet build "Passenger.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Passenger.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Passenger.API.dll"]
