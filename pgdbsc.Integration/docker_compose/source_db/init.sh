#!/bin/bash
set -e

# Run this on container init

# create user
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    CREATE USER dbsc_test_user CREATEDB PASSWORD 'testpw_source';
EOSQL

# create source db 1
PGPASSWORD=testpw_source psql -v ON_ERROR_STOP=1 --username "dbsc_test_user" --dbname "$POSTGRES_DB" <<-EOSQL
    CREATE DATABASE pgdbsc_test_source;
EOSQL

# init source db 1
PGPASSWORD=testpw_source psql -v ON_ERROR_STOP=1 --username "dbsc_test_user"  --dbname pgdbsc_test_source -f /setup_source_database.sql

# create source db 2
PGPASSWORD=testpw_source psql -v ON_ERROR_STOP=1 --username "dbsc_test_user" --dbname "$POSTGRES_DB" <<-EOSQL
    CREATE DATABASE pgdbsc_test_source_2;
EOSQL

# init source db 2
PGPASSWORD=testpw_source psql -v ON_ERROR_STOP=1 --username "dbsc_test_user" --dbname pgdbsc_test_source_2 -f /setup_source_database_2.sql
