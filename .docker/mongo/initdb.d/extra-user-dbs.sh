#!/bin/bash
# Copyright (c) Bentley Systems, Incorporated. All rights reserved.
set -euo pipefail

while IFS=":" read -r extra_name extra_password; do
  echo "extra_name=$extra_name"
  echo "extra_password=$extra_password"
  extra_name="$extra_name" extra_password="$extra_password" \
    mongosh \
    --username "$MONGO_INITDB_ROOT_USERNAME" \
    --password "$MONGO_INITDB_ROOT_PASSWORD" \
    admin \
    --eval 'db.createUser({user: process.env.extra_name, pwd: process.env.extra_password, roles:[{db: process.env.extra_name, role:"dbOwner"}]})'
done < <(tr '|' '\n' <<<"$EXTRA_USER_DBS")
