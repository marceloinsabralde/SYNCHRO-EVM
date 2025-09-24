#!/bin/bash
# Copyright (c) Bentley Systems, Incorporated. All rights reserved.
set -euo pipefail

try_until="$(( $(date +%s) + 10 ))"
while ! pg_isready -U "$POSTGRES_USER"; do
  if [ "$(date +%s)" -ge "$try_until" ]; then
    exit 1
  fi

  sleep 0.25
done
