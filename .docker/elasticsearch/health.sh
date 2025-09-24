#!/bin/sh
# Copyright (c) Bentley Systems, Incorporated. All rights reserved.
set -euo pipefail

try_until="$(( $(date +%s) + 30 ))"
while ! curl -fs -u "elastic:${ELASTIC_PASSWORD}" http://localhost:9200/_cluster/health > /dev/null; do
  if [ "$(date +%s)" -ge "$try_until" ]; then
    exit 1
  fi
  sleep 1
done
