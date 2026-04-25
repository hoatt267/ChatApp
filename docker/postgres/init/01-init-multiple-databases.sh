#!/usr/bin/env bash
set -euo pipefail

for db in "${IDENTITY_DB_NAME:-identity_db}" "${USER_DB_NAME:-user_db}" "${CHAT_DB_NAME:-chat_db}"; do
  if psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "postgres" -tAc "SELECT 1 FROM pg_database WHERE datname='${db}'" | grep -q 1; then
    echo "Database ${db} already exists, skipping."
  else
    echo "Creating database ${db}..."
    psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "postgres" -c "CREATE DATABASE \"${db}\";"
  fi
done
