FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Oip.Security.Api/Oip.Security.Api.csproj", "src/Oip.Security.Api/"]
COPY ["src/Oip.Security.EntityFramework.MySql/Oip.Security.EntityFramework.MySql.csproj", "src/Oip.Security.EntityFramework.MySql/"]
COPY ["src/Oip.Security.EntityFramework.Identity/Oip.Security.EntityFramework.Identity.csproj", "src/Oip.Security.EntityFramework.Identity/"]
COPY ["src/Oip.Security.EntityFramework/Oip.Security.EntityFramework.csproj", "src/Oip.Security.EntityFramework/"]
COPY ["src/Oip.Security.EntityFramework.Extensions/Oip.Security.EntityFramework.Extensions.csproj", "src/Oip.Security.EntityFramework.Extensions/"]
COPY ["src/Oip.Security.EntityFramework.Shared/Oip.Security.EntityFramework.Shared.csproj", "src/Oip.Security.EntityFramework.Shared/"]
COPY ["src/Oip.Security.EntityFramework.Configuration/Oip.Security.EntityFramework.Configuration.csproj", "src/Oip.Security.EntityFramework.Configuration/"]
COPY ["src/Oip.Security.Bl/Oip.Security.Bl.csproj", "src/Oip.Security.Bl/"]
COPY ["src/Oip.Security.Bl.Shared/Oip.Security.Bl.Shared.csproj", "src/Oip.Security.Bl.Shared/"]
COPY ["src/Oip.Security.EntityFramework.PostgreSQL/Oip.Security.EntityFramework.PostgreSQL.csproj", "src/Oip.Security.EntityFramework.PostgreSQL/"]
COPY ["src/Oip.Security.Shared.Configuration/Oip.Security.Shared.Configuration.csproj", "src/Oip.Security.Shared.Configuration/"]
COPY ["src/Oip.Security.Bl.Identity/Oip.Security.Bl.csproj", "src/Oip.Security.Bl.Identity/"]
COPY ["src/Oip.Security.Shared/Oip.Security.Shared.csproj", "src/Oip.Security.Shared/"]
COPY ["src/Oip.Security.EntityFramework.SqlServer/Oip.Security.EntityFramework.SqlServer.csproj", "src/Oip.Security.EntityFramework.SqlServer/"]
RUN dotnet restore "src/Oip.Security.Api/Oip.Security.Api.csproj"
COPY . .
WORKDIR "/src/src/Oip.Security.Api"
RUN dotnet build "Oip.Security.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Oip.Security.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
ENTRYPOINT ["dotnet", "Oip.Security.Api.dll"]