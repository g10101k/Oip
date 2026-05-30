ARG BUILD_PLATFORM=linux/amd64
FROM --platform=$BUILD_PLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY . .
RUN dotnet restore "./Oip.Applications/Oip.Applications.csproj"

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Oip.Applications/Oip.Applications.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 5008
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Oip.Applications.dll"]
