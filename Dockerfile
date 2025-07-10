# syntax=docker/dockerfile:1.8-labs
# check=error=true
# Copyright (c) Bentley Systems, Incorporated. All rights reserved.

FROM mcr.microsoft.com/dotnet/sdk:9.0-noble AS sdk
FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled AS base

FROM sdk AS src
WORKDIR /src
SHELL ["/bin/bash", "-euo", "pipefail", "-c"]

COPY \
  --link \
  --exclude=*.TestCommon \
  --exclude=*.Tests \
  --exclude=*.Scenarios \
  . .

RUN <<SH
  perl -Mwarnings=FATAL -i -ne '
    if (/^Project\(.*\) = .*?, "(.*?)",/) {
      $file = $1;
      $file =~ s|\\|/|g;
      if (! -f $file) {
        $skip = 1;
        next;
      }
    }
    if ($skip && /^EndProject/) {
      $skip = 0;
      next;
    }
    next if $skip;
    print;
  ' -- *.sln
SH

FROM sdk AS build
WORKDIR /src
SHELL ["/bin/bash", "-euo", "pipefail", "-c"]

RUN \
  --mount=type=cache,id=apt-cache,sharing=locked,target=/var/cache/apt \
  --mount=type=cache,id=apt-lib,sharing=locked,target=/var/lib/apt \
  <<SH
  rm /etc/apt/apt.conf.d/docker-clean
  echo 'Binary::apt::APT::Keep-Downloaded-Packages "true";' > /etc/apt/apt.conf.d/keep-cache
  apt-get update --yes

  DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
    rmlint=2.*
SH

RUN curl -sL https://aka.ms/install-artifacts-credprovider.sh | sh

COPY \
  --link \
  --from=src \
  --parents \
  --exclude=/src/*/*.TestCommon.csproj \
  --exclude=/src/*/*.Tests.csproj \
  --exclude=/src/*/*.Scenarios.csproj \
  /src/Directory.*.props \
  /src/nuget.config \
  /src/*.sln \
  /src/*/*.csproj \
  /

RUN \
  --mount=type=cache,id=nuget-packages,target=/mnt/nuget-packages \
  --mount=type=secret,id=FEED_ACCESS_TOKEN \
  <<SH
  FEED_ACCESS_TOKEN="$(cat /run/secrets/FEED_ACCESS_TOKEN)"
  VSS_NUGET_EXTERNAL_FEED_ENDPOINTS=$(cat <<EOF
  {
    "endpointCredentials": [
      {
        "endpoint": "https://pkgs.dev.azure.com/bentleycs/_packaging/Packages/nuget/v3/index.json",
        "username": "docker",
        "password": "$FEED_ACCESS_TOKEN"
      }
    ]
  }
EOF
  )
  export VSS_NUGET_EXTERNAL_FEED_ENDPOINTS

  ln -s /mnt/nuget-packages /root/.nuget/packages
  dotnet restore
  rm /root/.nuget/packages
  cp --archive /mnt/nuget-packages /root/.nuget/packages

  rm -r /root/.nuget/plugins/netcore/CredentialProvider.Microsoft
SH

COPY --link --from=src /src /src

ARG Version=0.0.0.0

RUN dotnet build --no-restore --configuration Release -p:Version="$Version"

FROM build AS publish
WORKDIR /src
SHELL ["/bin/bash", "-euo", "pipefail", "-c"]

RUN \
  <<SH
  mapfile -t project_names < <(
    dotnet sln list | grep / | cut -d / -f 1 | grep -v Common$
  )

  for project_name in "${project_names[@]}"; do
    dotnet publish "$project_name" --no-build --configuration Release --output "/app/$project_name"
  done

  find /app -type f -name '*.dll' | rmlint -c sh:hardlink -o sh:/app/rmlint.sh -
  /app/rmlint.sh -dq
SH

FROM base AS app
ENV DOTNET_EnableDiagnostics=0
COPY --link --from=publish /app /app
