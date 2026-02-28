# .NET Setup and Verification

This repository now includes repeatable scripts for installing and testing .NET 8 on Ubuntu 24.04.

## Install .NET 8 SDK

```bash
sudo bash scripts/install-dotnet.sh
```

The installer script:
- validates it is running as root,
- installs prerequisites,
- configures the Microsoft package feed (if missing),
- installs `dotnet-sdk-8.0`,
- prints `dotnet --info` at the end.

## Run smoke tests

```bash
bash scripts/test-dotnet.sh
```

The test script:
- checks `dotnet` is available,
- creates a temporary xUnit project,
- runs `dotnet test -v minimal`,
- cleans up temporary files.
