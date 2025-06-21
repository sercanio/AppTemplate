FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/AppTemplate.Web/AppTemplate.Web.csproj", "src/AppTemplate.Web/"]
COPY ["src/AppTemplate.Application/AppTemplate.Application.csproj", "src/AppTemplate.Application/"]
COPY ["src/AppTemplate.Domain/AppTemplate.Domain.csproj", "src/AppTemplate.Domain/"]
COPY ["src/AppTemplate.Infrastructure/AppTemplate.Infrastructure.csproj", "src/AppTemplate.Infrastructure/"]
COPY ["core/", "core/"]
RUN dotnet restore "src/AppTemplate.Web/AppTemplate.Web.csproj"
COPY . .
WORKDIR "/src/src/AppTemplate.Web"
RUN dotnet build "AppTemplate.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AppTemplate.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AppTemplate.Web.dll"]