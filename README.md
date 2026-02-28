# BaileysSharp

BaileysSharp is a .NET 8 scaffold for porting core patterns from [WhiskeySockets/Baileys](https://github.com/WhiskeySockets/Baileys) into C#.

## Prerequisites

- .NET 8 SDK (`8.0.x`)

Example install command (Linux/macOS):

```bash
curl -fsSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0
export PATH="$HOME/.dotnet:$PATH"
```

## Run tests

```bash
dotnet test BaileysSharp.sln -v minimal
```

## Included components

- Auth state contracts and implementations (`AuthCreds`, `IAuthStateStore`, `InMemoryAuthStateStore`) with defensive copying.
- A `ClientWebSocket` wrapper (`BaileysSocket`) with async connect/send flow and safer receive-loop behavior.
- `BufferJson` helper for Baileys-style buffer JSON serialization/revival behavior.
- xUnit tests for auth state, socket behavior/options, and buffer JSON handling.

## Project layout

- `src/BaileysSharp/Auth` - auth and credential state handling.
- `src/BaileysSharp/Models` - typed domain models.
- `src/BaileysSharp/Socket` - socket options, contracts, and runtime implementation.
- `src/BaileysSharp/Utils` - utility helpers such as buffer JSON conversion.
- `tests/BaileysSharp.Tests` - unit tests.
