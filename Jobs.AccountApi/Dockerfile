﻿# Password for the certificate
ARG CERT_PASSWORD_ARG=12345

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine-amd64 AS build-env
ARG BUILD_CONFIGURATION=Release
ARG CERT_PASSWORD_ARG
ENV CERT_PASSWORD=$CERT_PASSWORD_ARG
WORKDIR .

COPY ["Jobs.AccountApi/Jobs.AccountApi.csproj", "Jobs.AccountApi/"]
COPY ["Jobs.Common/Jobs.Common.csproj", "Jobs.Common/"]
COPY ["Jobs.Core/Jobs.Core.csproj", "Jobs.Core/"]
COPY ["Jobs.Dto/Jobs.Dto.csproj", "Jobs.Dto/"]
COPY ["Jobs.Entities/Jobs.Entities.csproj", "Jobs.Entities/"]

COPY ["dev-cert/server.cert.pem", "Jobs.AccountApi/dev-cert/server.cert.pem"]
COPY ["dev-cert/server.key.pem", "Jobs.AccountApi/dev-cert/server.key.pem"]

RUN dotnet restore "Jobs.AccountApi/Jobs.AccountApi.csproj"
COPY . .
WORKDIR "/Jobs.AccountApi"
RUN dotnet build "Jobs.AccountApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

RUN apk add openssl

# Make the directory for certificate export
RUN mkdir /config

#WORKDIR .

COPY ["dev-cert/server.cert.pem", "/config/cert.pem"]
COPY ["dev-cert/server.key.pem", "/config/key.pem"]

# Generate certificate with specified password
# RUN openssl req -x509 -newkey rsa:4096 -keyout  /config/key.pem -out  /config/cert.pem -days 9999 -passout pass:$CERT_PASSWORD -subj "/C=UA/ST=Mykolayiv/L=Vishneva/O=/OU=/CN=localhost" -addext subjectAltName="DNS:localhost, IP:127.0.0.1, IP:::1, DNS:host.docker.internal, DNS:host.containers.internal"

FROM build-env AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Jobs.AccountApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# copy generated certificate
COPY --from=build-env /config /app/publish/config

# this image contains the ASP.NET Core and .NET runtimes and libraries 
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine-amd64
ARG CERT_PASSWORD_ARG
ENV CERT_PASSWORD=$CERT_PASSWORD_ARG
WORKDIR /app
# add dependency in system to setup certificates
RUN apk add ca-certificates 
# create directory to store certificate config
RUN mkdir /config 
# create necessary config directory
RUN mkdir -p /usr/local/share/ca-certificates/
# copy compiled files to runtime
# COPY --from=build-env /App/out . 
COPY --from=publish /app/publish .
# copy generated certificate
#COPY --from=build-env /config /config
# Disable Big Brother
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
# Set the environment to production
ENV ASPNETCORE_ENVIRONMENT=Docker
# Set the urls where Kestrel is going to listen
ENV ASPNETCORE_URLS=https://+:7161
# location of the certificate file
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/usr/local/share/ca-certificates/vacancy-api-cert.crt
# location of the certificate key
ENV ASPNETCORE_Kestrel__Certificates__Default__KeyPath=/usr/local/share/ca-certificates/vacancy-api-cert.key
# specify password in order to open certificate key
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=$CERT_PASSWORD
#ENV ASPNETCORE_Kestrel__Certificates__Default__Password=dfvgbh
# copy certificate files to config directory
RUN cp ./config/cert.pem $ASPNETCORE_Kestrel__Certificates__Default__Path 
RUN cp ./config/key.pem $ASPNETCORE_Kestrel__Certificates__Default__KeyPath
# set file permisions for certificate file
RUN chmod 644 $ASPNETCORE_Kestrel__Certificates__Default__Path 
RUN chmod +x $ASPNETCORE_Kestrel__Certificates__Default__Path
# change file ownership for certificate file
# add generated certificate to trusted certificate list on the system
RUN cat $ASPNETCORE_Kestrel__Certificates__Default__Path >> /etc/ssl/certs/ca-certificates.crt
# set file permissions for key file
RUN chmod 644 $ASPNETCORE_Kestrel__Certificates__Default__KeyPath
RUN chmod +x $ASPNETCORE_Kestrel__Certificates__Default__KeyPath
# change file ownership for key file
RUN update-ca-certificates

USER $APP_UID
WORKDIR /app
EXPOSE 7161
ENTRYPOINT ["dotnet", "Jobs.AccountApi.dll"]
