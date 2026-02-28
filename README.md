# BaileysSharp

BaileysSharp is a small .NET 8 scaffold for porting parts of the [WhiskeySockets/Baileys](https://github.com/WhiskeySockets/Baileys) behavior into C#.

## Included components

- Auth state contracts and an in-memory implementation (`AuthCreds`, `IAuthStateStore`, `InMemoryAuthStateStore`).
- A `ClientWebSocket` wrapper (`BaileysSocket`) with safer receive-loop handling.
- `BufferJson` helper to mimic Baileys buffer JSON serialization/revival behavior.
- xUnit tests, including C# conversions of upstream Baileys tests around buffer JSON handling.

## Run tests

```bash
dotnet test BaileysSharp.sln -v minimal
```
