FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Analytics.Worker/Analytics.Worker.csproj", "Analytics.Worker/"]
COPY ["Shared.Messages/Shared.Messages.csproj", "Shared.Messages/"]
RUN dotnet restore "Analytics.Worker/Analytics.Worker.csproj"
COPY . .
WORKDIR "/src/Analytics.Worker"
RUN dotnet build "Analytics.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Analytics.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Analytics.Worker.dll"]
