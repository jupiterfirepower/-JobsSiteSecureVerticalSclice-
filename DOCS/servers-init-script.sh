#!/bin/bash

export OPENSSL_PASSWORD='paramount'
export JOB_PROJECT_ROOT_DIR=$HOME/RiderProjects/SecureVerticalSlice/JobProject

if [ ! -d "$HOME/vault" ]; then
  echo "Directory '$HOME/vault' does not exist. Creating it."
  mkdir -p $HOME/vault/{config,file,logs,data,certs,pki,scripts}
  #docker exec -it vault vault operator init -key-shares=1 -key-threshold=3 -format=json > $HOME/vault/config/vault-cluster.json

  cat << 'EOF' > $HOME/vault/config/config.hcl
default_lease_ttl = "168h"
max_lease_ttl = "720h"
ui = true
api_addr = "http://127.0.0.1:8200"
plugin_directory = "/vault/plugins"
log_level = "Debug"

storage "file" {
  path = "/vault/file"
}

listener "tcp" {
  address = "0.0.0.0:8200"
  tls_disable = 1
  tls_cert_file = "/vault/certs/server.vault.crt"
  tls_key_file = "/vault/certs/server.vault.key"
}
EOF

  cat << 'EOF' > $HOME/vault/config/configcert.hcl
default_lease_ttl = "168h"
max_lease_ttl = "720h"
ui = true
api_addr = "https://127.0.0.1:8200"
plugin_directory = "/vault/plugins"
log_level = "Debug"

storage "file" {
  path = "/vault/file"
}

listener "tcp" {
  address = "0.0.0.0:8200"
  tls_disable = 0
  tls_cert_file = "/vault/certs/server.vault.crt"
  tls_key_file = "/vault/certs/server.vault.key"
}
EOF

  cat << 'EOF' > $HOME/vault/scripts/vault-init.sh
#!/bin/sh

set -ex
apk add jq

echo "Unsealing Vault"
export VAULT_TOKEN=$(jq .root_token /vault/scripts/vault-cluster.json  | awk '/^[^][]/{print $1}' | sed 's/\"//g')
vault operator unseal $(jq .keys_base64 /vault/scripts/vault-cluster.json  | awk '/^[^][]/{print $1}' | sed 's/\"//g')

vault status

PKI_INIT_FILE=/vault/pki/pki.init
if [[ -f "${PKI_INIT_FILE}" ]]; then
   echo "${PKI_INIT_FILE} exists. Vault pki already initialized."
else
  ## Enable PKI Secrets Engine
  vault secrets enable pki
  ## Tune Max Lease TTL for PKI Secrets Engine
  vault secrets tune -max-lease-ttl=876000h pki
  ## Generate Root Certificate
  vault write pki/root/generate/internal common_name=mydomain.io ttl=876000h
  ## Write Root Certificate to File
  vault write -field=certificate pki/root/generate/internal common_name="mydomain-local" issuer_name="vault-pki" ttl=876000h > /vault/pki/vault_root_ca.crt
  ## Create Certificate Role
  vault write pki/roles/mydomain-local allow_any_name=true
  ## Configure PKI URLs
  vault write pki/config/urls issuing_certificates="http://vault:8200/v1/pki/ca" crl_distribution_points="http://vault:8200/v1/pki/crl"

  ## Enable Intermediate PKI Secrets Engine
  vault secrets enable -path=pki_int pki
  ## Tune Max Lease TTL for Intermediate PKI Secrets Engine
  vault secrets tune -max-lease-ttl=876000h pki_int
  ## Generate Intermediate CSR (Certificate Signing Request)
  vault write -field=csr pki_int/intermediate/generate/internal common_name="MyDomain Local Intermediate Authority" issuer_name="mydomain-local-intermediate" > /vault/pki/pki_intermediate.csr
  ## Display Intermediate CSR
  cat /vault/pki/pki_intermediate.csr
  ## Sign Intermediate CSR with Root Certificate
  vault write -field=certificate pki/root/sign-intermediate issuer_ref="vault-pki" csr=@/vault/pki/pki_intermediate.csr format=pem_bundle ttl="876000h" > /vault/pki/intermediate.cert.pem
  ## Set Signed Intermediate Certificate
  vault write pki_int/intermediate/set-signed certificate=@/vault/pki/intermediate.cert.pem

  ## Create server role
  vault write pki_int/roles/server issuer_ref="$(vault read -field=default pki_int/config/issuers)" allowed_domains=localhost,127.0.0.1,host.docker.internal allow_subdomains=true allow_bare_domains=true require_cn=false server_flag=true max_ttl=8670h

  ## Create client role
  vault write pki_int/roles/client issuer_ref="$(vault read -field=default pki_int/config/issuers)" require_cn=false client_flag=true allow_any_name=true max_ttl=8670h

  touch ${PKI_INIT_FILE}
fi
EOF

chmod +x $HOME/vault/scripts/vault-init.sh

  cat << 'EOF' > $HOME/vault/scripts/vault-init-cert.sh
#!/bin/sh

set -ex
apk add jq

echo "Unsealing Vault"
export VAULT_TOKEN=$(jq .root_token /vault/scripts/vault-cluster.json  | awk '/^[^][]/{print $1}' | sed 's/\"//g')
vault operator unseal $(jq .keys_base64 /vault/scripts/vault-cluster.json  | awk '/^[^][]/{print $1}' | sed 's/\"//g')

vault status
EOF

chmod +x $HOME/vault/scripts/vault-init-cert.sh

cat << 'EOF' > $HOME/vault/config/vault-cluster.json
{
  "keys": [
    "1ce21e576387e46651f37a011b6975a6048c8cca41cad533c20035a15e9d43a0"
  ],
  "keys_base64": [
    "HOIeV2OH5GZR83oBG2l1pgSMjMpBytUzwgA1oV6dQ6A="
  ],
  "root_token": "???????"
}
EOF

else
  echo "Directory '$HOME/vault' exists. Skip."
fi

#openssl genrsa -aes-256-cbc -out rootCA.key 4096

if [ ! -d "$JOB_PROJECT_ROOT_DIR/devcerts" ]; then
   echo "Directory '$JOB_PROJECT_ROOT_DIR/devcerts' does not exist. Creating it."
   mkdir -p $JOB_PROJECT_ROOT_DIR/devcerts/dev/{vault,consul,keycloak,service}
   cp -a $JOB_PROJECT_ROOT_DIR/DOCS/devcerts/service/. $JOB_PROJECT_ROOT_DIR/devcerts/dev/service/

   cd $JOB_PROJECT_ROOT_DIR/devcerts/dev

   openssl genrsa -aes256 -passout env:OPENSSL_PASSWORD -out rootCA.key 4096
   openssl req -x509 -new -nodes -key rootCA.key -sha256 -days 9999 -out rootCA.crt -subj "/C=UA/ST=MR/L=Mykolayiv/CN=rootCA" -passin env:OPENSSL_PASSWORD

   cp $JOB_PROJECT_ROOT_DIR/devcerts/dev/rootCA.crt $HOME/vault/certs/server.rootCA.crt
   cp $JOB_PROJECT_ROOT_DIR/devcerts/dev/rootCA.key $HOME/vault/certs/server.rootCA.key

   cd $JOB_PROJECT_ROOT_DIR/devcerts/dev/vault

   openssl genrsa -out server.vault.key 4096
   openssl req -new -sha256 -key server.vault.key -subj "/C=UA/ST=MK/L=Mykolayiv/CN=vault" -out server.vault.csr -addext "subjectAltName=DNS:vault,DNS:localhost,DNS:host.docker.internal,IP:127.0.0.1"
   openssl x509 -req -extfile <(printf "subjectAltName=DNS:vault,DNS:localhost,DNS:host.docker.internal,IP:127.0.0.1") -in server.vault.csr -CA ../rootCA.crt -CAkey ../rootCA.key -CAcreateserial -out server.vault.crt -days 9999 -sha256 -passin env:OPENSSL_PASSWORD

   cp $JOB_PROJECT_ROOT_DIR/devcerts/dev/vault/server.vault.crt $HOME/vault/certs
   cp $JOB_PROJECT_ROOT_DIR/devcerts/dev/vault/server.vault.key $HOME/vault/certs

   cd $JOB_PROJECT_ROOT_DIR/devcerts/dev/consul
   openssl genrsa -out server.consul.key 4096
   openssl req -new -sha256 -key server.consul.key -subj "/C=UA/ST=MK/L=Mykolayiv/CN=consul" -out server.consul.csr -addext "subjectAltName=DNS:consul,DNS:localhost,DNS:host.docker.internal,IP:127.0.0.1"
   openssl x509 -req -extfile <(printf "subjectAltName=DNS:consul,DNS:localhost,DNS:host.docker.internal,IP:127.0.0.1") -in server.consul.csr -CA ../rootCA.crt -CAkey ../rootCA.key -CAcreateserial -out server.consul.crt -days 9999 -sha256 -passin env:OPENSSL_PASSWORD

   cd $JOB_PROJECT_ROOT_DIR/devcerts/dev/keycloak

   openssl genrsa -out server.keycloak.key 4096
   openssl req -new -sha256 -key server.keycloak.key -subj "/C=UA/ST=MK/L=Mykolayiv/CN=keycloak" -out server.keycloak.csr -addext "subjectAltName=DNS:keycloak,DNS:localhost,DNS:host.docker.internal,IP:127.0.0.1"
   openssl x509 -req -extfile <(printf "subjectAltName=DNS:keycloak,DNS:localhost,DNS:host.docker.internal,IP:127.0.0.1") -in server.keycloak.csr -CA ../rootCA.crt -CAkey ../rootCA.key -CAcreateserial -out server.keycloak.crt -days 9999 -sha256 -passin env:OPENSSL_PASSWORD
else
   echo "Directory '$JOB_PROJECT_ROOT_DIR/devcerts' exists. Skip."
fi
