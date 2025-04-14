FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 7007

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Jobs.ReferenceApi/Jobs.ReferenceApi.csproj", "Jobs.ReferenceApi/"]
COPY ["Jobs.Common/Jobs.Common.csproj", "Jobs.Common/"]
COPY ["Jobs.Core/Jobs.Core.csproj", "Jobs.Core/"]
COPY ["Jobs.Dto/Jobs.Dto.csproj", "Jobs.Dto/"]
COPY ["Jobs.Entities/Jobs.Entities.csproj", "Jobs.Entities/"]
RUN dotnet restore "Jobs.ReferenceApi/Jobs.ReferenceApi.csproj"
COPY . .
WORKDIR "/src/Jobs.ReferenceApi"
RUN dotnet build "Jobs.ReferenceApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Jobs.ReferenceApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Jobs.ReferenceApi.dll"]
