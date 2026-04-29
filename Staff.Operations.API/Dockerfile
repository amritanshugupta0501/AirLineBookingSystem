FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Staff.Operations.API/Staff.Operations.API.csproj", "Staff.Operations.API/"]
RUN dotnet restore "Staff.Operations.API/Staff.Operations.API.csproj"
COPY . .
WORKDIR "/src/Staff.Operations.API"
RUN dotnet build "Staff.Operations.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Staff.Operations.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Staff.Operations.API.dll"]
