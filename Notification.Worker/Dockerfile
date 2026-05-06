FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore
COPY ["Notification.Worker/Notification.Worker.csproj", "Notification.Worker/"]
COPY ["Shared.Messages/Shared.Messages.csproj", "Shared.Messages/"]
RUN dotnet restore "Notification.Worker/Notification.Worker.csproj"

# Copy the remaining files and build
COPY . .
WORKDIR "/src/Notification.Worker"
RUN dotnet publish "Notification.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Generate runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Notification.Worker.dll"]
