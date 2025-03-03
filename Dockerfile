# Copyright (c) Bentley Systems, Incorporated. All rights reserved.
FROM mcr.microsoft.com/dotnet/sdk:9.0

RUN dotnet tool install dotnet-ef --version 9.0.2 --tool-path /usr/local/bin

RUN \
  apt-get update --yes && \
  DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
    lsb-release ca-certificates gnupg wget curl && \
  curl -fsSL https://www.postgresql.org/media/keys/ACCC4CF8.asc | gpg --dearmor -o /usr/share/keyrings/postgresql.gpg && \
  echo "deb [signed-by=/usr/share/keyrings/postgresql.gpg] http://apt.postgresql.org/pub/repos/apt/ $(lsb_release -cs)-pgdg main" > /etc/apt/sources.list.d/pgdg.list && \
  apt-get update --yes && \
  DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
    postgresql-client-16

WORKDIR /app
ADD . .
