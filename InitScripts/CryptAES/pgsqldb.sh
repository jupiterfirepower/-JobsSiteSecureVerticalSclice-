#!/usr/bin/bash

# yes | dnf upgrade
sudo dnf install postgresql -y
export PGPASSWORD='newpwd'; psql -h localhost -U admin -c 'create database keycloak;'
export PGPASSWORD='newpwd'; psql -h localhost -U admin -c 'create database jobs_db;'