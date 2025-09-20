FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 50002

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Oip.Discussion/Oip.Discussion.csproj", "Oip.Discussion/"]
RUN dotnet restore "Oip.Discussion/Oip.Discussion.csproj"
COPY . .
WORKDIR "/src/Oip.Discussion"
RUN dotnet build "./Oip.Discussion.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Oip.Discussion.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Oip.Discussion.dll"]
