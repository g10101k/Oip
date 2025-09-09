FROM node:22-alpine AS angular-build
WORKDIR /src
COPY . .
WORKDIR /src/Oip.WebClient
RUN npm install
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY . .
RUN dotnet restore "./Oip/Oip.csproj"

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Oip/Oip.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0  AS base
WORKDIR /app
EXPOSE 50000

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=angular-build /src/Oip.WebClient/dist/oip ./wwwroot
ENTRYPOINT ["dotnet", "Oip.dll" ]