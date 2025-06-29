#!/bin/bash
# Copyright (c) Bentley Systems, Incorporated. All rights reserved.
set -euo pipefail

while IFS=":" read -r extra_name extra_password extra_db; do
  : ${extra_db:=$extra_name}

  psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" -v extra_name="$extra_name" -v extra_password="$extra_password" <<SQL
    CREATE USER :"extra_name" ENCRYPTED PASSWORD :'extra_password';
SQL

  psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" -v extra_name="$extra_name" -v extra_db="$extra_db" <<SQL
    CREATE DATABASE :"extra_db" WITH OWNER :"extra_name";
SQL
done < <(tr '|' '\n' <<<"$EXTRA_USER_DBS")
