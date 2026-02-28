namespace BaileysSharp.Socket;

public sealed class BaileysSocketOptions
{
    public Uri WebSocketEndpoint { get; init; } = new("wss://web.whatsapp.com/ws/chat");
    public TimeSpan KeepAliveInterval { get; init; } = TimeSpan.FromSeconds(20);
    public string BrowserDescription { get; init; } = "BaileysSharp/0.1";

    public void Validate()
    {
        if (!WebSocketEndpoint.Scheme.Equals("wss", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("WhatsApp endpoint must be a secure WebSocket URL.");

        if (KeepAliveInterval <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(KeepAliveInterval));
    }
}
