# Password for the certificate
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

COPY ./devcerts/*  /Jobs.ReferenceApi/

RUN dotnet restore "Jobs.AccountApi/Jobs.AccountApi.csproj"
COPY . .
WORKDIR "/Jobs.AccountApi"
RUN dotnet build "Jobs.AccountApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

RUN apk add openssl

# Make the directory for certificate export
RUN mkdir /config

#WORKDIR .

COPY ./devcerts/dev/service/server.cert.pem /config/service.cert.pem
COPY ./devcerts/dev/service/server.key.pem /config/service.key.pem

COPY ./devcerts/dev/rootCA.crt /config/server.rootCA.crt
COPY ./devcerts/dev/rootCA.key /config/server.rootCA.key

COPY ./devcerts/dev/vault/server.vault.crt /config/server.vault.crt
COPY ./devcerts/dev/vault/server.vault.key /config/server.vault.key

COPY ./devcerts/dev/consul/server.consul.crt /config/server.consul.crt
COPY ./devcerts/dev/consul/server.consul.key /config/server.consul.key

COPY ./devcerts/dev/keycloak/server.keycloak.crt /config/server.keycloak.crt
COPY ./devcerts/dev/keycloak/server.keycloak.key /config/server.keycloak.key

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
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/usr/local/share/ca-certificates/service.cert.crt
# location of the certificate key
ENV ASPNETCORE_Kestrel__Certificates__Default__KeyPath=/usr/local/share/ca-certificates/service.cert.key
# specify password in order to open certificate key
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=$CERT_PASSWORD
#ENV ASPNETCORE_Kestrel__Certificates__Default__Password=dfvgbh
# copy certificate files to config directory
RUN cp ./config/service.cert.pem $ASPNETCORE_Kestrel__Certificates__Default__Path 
RUN cp ./config/service.key.pem $ASPNETCORE_Kestrel__Certificates__Default__KeyPath

# set file permisions for certificate file
RUN chmod 644 $ASPNETCORE_Kestrel__Certificates__Default__Path 
# set file permissions for key file
RUN chmod 644 $ASPNETCORE_Kestrel__Certificates__Default__KeyPath

# RootCA for Vault, Consul, Keycloak
RUN cp ./config/server.rootCA.crt /usr/local/share/ca-certificates/server.rootCA.crt
RUN cp ./config/server.rootCA.key /usr/local/share/ca-certificates/server.rootCA.key
# set file permisions for certificate file
RUN chmod 644 /usr/local/share/ca-certificates/server.rootCA.crt
# set file permissions for key file
RUN chmod 644 /usr/local/share/ca-certificates/server.rootCA.key

# Vault
RUN cp ./config/server.vault.crt /usr/local/share/ca-certificates/server.vault.crt
RUN cp ./config/server.vault.key /usr/local/share/ca-certificates/server.vault.key
# set file permisions for certificate file
RUN chmod 644 /usr/local/share/ca-certificates/server.vault.crt
# set file permissions for key file
RUN chmod 644 /usr/local/share/ca-certificates/server.vault.key

# Consul
RUN cp ./config/server.consul.crt /usr/local/share/ca-certificates/server.consul.crt
RUN cp ./config/server.consul.key /usr/local/share/ca-certificates/server.consul.key
# set file permisions for certificate file
RUN chmod 644 /usr/local/share/ca-certificates/server.consul.crt
# set file permissions for key file
RUN chmod 644 /usr/local/share/ca-certificates/server.consul.key

# Keycloak
RUN cp ./config/server.keycloak.crt /usr/local/share/ca-certificates/server.keycloak.crt
RUN cp ./config/server.keycloak.key /usr/local/share/ca-certificates/server.keycloak.key
# set file permisions for certificate file
RUN chmod 644 /usr/local/share/ca-certificates/server.keycloak.crt
# set file permissions for key file
RUN chmod 644 /usr/local/share/ca-certificates/server.keycloak.key

# add generated certificate to trusted certificate list on the system
RUN cat $ASPNETCORE_Kestrel__Certificates__Default__Path >> /etc/ssl/certs/ca-certificates.crt

RUN cat /usr/local/share/ca-certificates/server.rootCA.crt >> /etc/ssl/certs/ca-certificates.crt

RUN cat /usr/local/share/ca-certificates/server.vault.crt >> /etc/ssl/certs/ca-certificates.crt

RUN cat /usr/local/share/ca-certificates/server.consul.crt >> /etc/ssl/certs/ca-certificates.crt

RUN cat /usr/local/share/ca-certificates/server.keycloak.crt >> /etc/ssl/certs/ca-certificates.crt

# change file ownership for key file
RUN update-ca-certificates

USER $APP_UID
WORKDIR /app
EXPOSE 7161
ENTRYPOINT ["dotnet", "Jobs.AccountApi.dll"]
