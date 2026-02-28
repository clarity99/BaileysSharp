#!/usr/bin/env bash
set -euo pipefail

if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet is not installed. Run scripts/install-dotnet.sh first." >&2
  exit 1
fi

workdir="$(mktemp -d /tmp/dotnet-smoke-XXXXXX)"
trap 'rm -rf "$workdir"' EXIT

pushd "$workdir" >/dev/null

dotnet --info
dotnet --list-sdks
dotnet new xunit -n SmokeTests >/dev/null
dotnet test SmokeTests/SmokeTests.csproj -v minimal

popd >/dev/null
