# BaileysSharp

`BaileysSharp` is a .NET-first starter port of the [WhiskeySockets/Baileys](https://github.com/WhiskeySockets/Baileys) approach for WhatsApp Web automation.

## Prerequisites

- .NET 8 SDK (`8.0.x`)

Example install command (Linux/macOS):

```bash
curl -fsSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0
export PATH="$HOME/.dotnet:$PATH"
```

Run the test suite:

```bash
dotnet test BaileysSharp.sln
```

## What is included

- `AuthCreds` and `IAuthStateStore` abstractions to model Baileys-style auth-state persistence.
- `BaileysSocket` wrapper over `ClientWebSocket` with:
  - async connect flow,
  - minimal JSON send API for text messages,
  - incoming message parsing into a typed `WaMessage` model,
  - event-based message delivery.
- xUnit test project with basic coverage around auth storage and socket options validation.

## Project layout

- `src/BaileysSharp/Auth` - auth and credential state handling.
- `src/BaileysSharp/Models` - typed domain models.
- `src/BaileysSharp/Socket` - socket options, contracts, and runtime implementation.
- `tests/BaileysSharp.Tests` - unit tests.

## Notes

This is a foundational conversion scaffold, not yet a full protocol-complete implementation of Baileys. Next steps would typically include:

1. Noise protocol handshake compatibility with WhatsApp Web.
2. Signal session and key-store lifecycle.
3. QR login and reconnect/state sync workflows.
4. Rich message/media, receipts, and app-state patch handling.
