namespace BaileysSharp.Models;

public sealed record WaMessage(
    string Id,
    string ChatJid,
    string SenderJid,
    string MessageType,
    string? Text,
    DateTimeOffset TimestampUtc);
