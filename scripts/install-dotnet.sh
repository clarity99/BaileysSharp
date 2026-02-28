#!/usr/bin/env bash
set -euo pipefail

if [[ "${EUID}" -ne 0 ]]; then
  echo "This script must be run as root (use sudo)." >&2
  exit 1
fi

if ! command -v apt-get >/dev/null 2>&1; then
  echo "apt-get not found; this installer currently supports Ubuntu/Debian only." >&2
  exit 1
fi

source /etc/os-release
if [[ "${ID:-}" != "ubuntu" || "${VERSION_ID:-}" != "24.04" ]]; then
  echo "Warning: script was validated on Ubuntu 24.04; detected ${PRETTY_NAME:-unknown}." >&2
fi

export DEBIAN_FRONTEND=noninteractive
apt-get update
apt-get install -y wget gpg apt-transport-https

if [[ ! -f /etc/apt/sources.list.d/microsoft-prod.list ]]; then
  wget -q https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O /tmp/packages-microsoft-prod.deb
  dpkg -i /tmp/packages-microsoft-prod.deb
fi

apt-get update
apt-get install -y dotnet-sdk-8.0

dotnet --info
