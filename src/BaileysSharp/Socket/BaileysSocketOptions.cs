namespace BaileysSharp.Socket;

public sealed class BaileysSocketOptions
{
    public Uri WebSocketEndpoint { get; set; } = new("wss://web.whatsapp.com/ws/chat");
    public TimeSpan KeepAliveInterval { get; set; } = TimeSpan.FromSeconds(20);
    public string BrowserDescription { get; set; } = "BaileysSharp/0.1";

    public void Validate()
    {
        if (WebSocketEndpoint.Scheme != "wss")
            throw new ArgumentException("WebSocket endpoint must use wss://", nameof(WebSocketEndpoint));

        if (KeepAliveInterval <= TimeSpan.Zero)
            throw new ArgumentException("KeepAliveInterval must be positive", nameof(KeepAliveInterval));

        ArgumentException.ThrowIfNullOrWhiteSpace(BrowserDescription);
    }
}
