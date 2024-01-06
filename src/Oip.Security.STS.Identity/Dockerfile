FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Oip.Security.STS.Identity/Oip.Security.STS.Identity.csproj", "src/Oip.Security.STS.Identity/"]
COPY ["src/Oip.Security.Shared.Configuration/Oip.Security.Shared.Configuration.csproj", "src/Oip.Security.Shared.Configuration/"]
COPY ["src/Oip.Security.EntityFramework.Shared/Oip.Security.EntityFramework.Shared.csproj", "src/Oip.Security.EntityFramework.Shared/"]
COPY ["src/Oip.Security.EntityFramework.Identity/Oip.Security.EntityFramework.Identity.csproj", "src/Oip.Security.EntityFramework.Identity/"]
COPY ["src/Oip.Security.EntityFramework/Oip.Security.EntityFramework.csproj", "src/Oip.Security.EntityFramework/"]
COPY ["src/Oip.Security.EntityFramework.Extensions/Oip.Security.EntityFramework.Extensions.csproj", "src/Oip.Security.EntityFramework.Extensions/"]
COPY ["src/Oip.Security.EntityFramework.Configuration/Oip.Security.EntityFramework.Configuration.csproj", "src/Oip.Security.EntityFramework.Configuration/"]
COPY ["src/Oip.Security.Shared/Oip.Security.Shared.csproj", "src/Oip.Security.Shared/"]
COPY ["src/Oip.Security.Bl.Identity/Oip.Security.Bl.csproj", "src/Oip.Security.Bl.Identity/"]
COPY ["src/Oip.Security.Bl.Shared/Oip.Security.Bl.Shared.csproj", "src/Oip.Security.Bl.Shared/"]
RUN dotnet restore "src/Oip.Security.STS.Identity/Oip.Security.STS.Identity.csproj"
COPY . .
WORKDIR "/src/src/Oip.Security.STS.Identity"
RUN dotnet build "Oip.Security.STS.Identity.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Oip.Security.STS.Identity.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
ENTRYPOINT ["dotnet", "Oip.Security.STS.Identity.dll"]