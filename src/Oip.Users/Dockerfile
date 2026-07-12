ARG BUILD_PLATFORM=linux/amd64
FROM --platform=$BUILD_PLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY . .
RUN dotnet restore "./Oip.Users/Oip.Users.csproj"

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Oip.Users/Oip.Users.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 5005
COPY --from=oip-dev-https oip-dev-ca.crt /usr/local/share/ca-certificates/oip-dev-ca.crt
RUN update-ca-certificates
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Oip.Users.dll"]
