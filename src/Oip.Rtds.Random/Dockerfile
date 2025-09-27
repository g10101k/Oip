FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Oip.Rtds.Random/Oip.Rtds.Random.csproj", "Oip.Rtds.Random/"]
RUN dotnet restore "Oip.Rtds.Random/Oip.Rtds.Random.csproj"
COPY . .
WORKDIR "/src/Oip.Rtds.Random"
RUN dotnet build "./Oip.Rtds.Random.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Oip.Rtds.Random.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Oip.Rtds.Random.dll"]
