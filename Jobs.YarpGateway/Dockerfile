FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 7045

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Jobs.YarpGateway/Jobs.YarpGateway.csproj", "Jobs.YarpGateway/"]
COPY ["Jobs.Common/Jobs.Common.csproj", "Jobs.Common/"]
RUN dotnet restore "Jobs.YarpGateway/Jobs.YarpGateway.csproj"
COPY . .
WORKDIR "/src/Jobs.YarpGateway"
RUN dotnet build "Jobs.YarpGateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Jobs.YarpGateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Jobs.YarpGateway.dll"]
