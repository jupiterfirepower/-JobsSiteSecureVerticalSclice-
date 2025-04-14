FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 7111

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR .

COPY ["Jobs.VacancyApi/Jobs.VacancyApi.csproj", "Jobs.VacancyApi/"]
COPY ["Jobs.Common/Jobs.Common.csproj", "Jobs.Common/"]
COPY ["Jobs.Core/Jobs.Core.csproj", "Jobs.Core/"]
COPY ["Jobs.Dto/Jobs.Dto.csproj", "Jobs.Dto/"]
COPY ["Jobs.Entities/Jobs.Entities.csproj", "Jobs.Entities/"]
RUN dotnet restore "Jobs.VacancyApi/Jobs.VacancyApi.csproj"
COPY . .
WORKDIR "/Jobs.VacancyApi"
RUN dotnet build "Jobs.VacancyApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Jobs.VacancyApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Jobs.VacancyApi.dll"]
