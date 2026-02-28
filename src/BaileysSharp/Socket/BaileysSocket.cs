using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using BaileysSharp.Auth;
using BaileysSharp.Models;

namespace BaileysSharp.Socket;

public sealed class BaileysSocket : IBaileysSocket
{
    private readonly ClientWebSocket _socket = new();
    private readonly IAuthStateStore _authStateStore;
    private readonly BaileysSocketOptions _options;
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);
    private Task? _receiveLoop;

    public event Func<WaMessage, ValueTask>? MessageReceived;

    public BaileysSocket(IAuthStateStore authStateStore, BaileysSocketOptions? options = null)
    {
        _authStateStore = authStateStore;
        _options = options ?? new BaileysSocketOptions();
        _options.Validate();

        _socket.Options.KeepAliveInterval = _options.KeepAliveInterval;
        _socket.Options.SetRequestHeader("User-Agent", _options.BrowserDescription);
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        var creds = await _authStateStore.ReadAsync(cancellationToken) ?? AuthCreds.CreateNew();
        await _authStateStore.WriteAsync(creds, cancellationToken);

        await _socket.ConnectAsync(_options.WebSocketEndpoint, cancellationToken);
        _receiveLoop = Task.Run(() => ReceiveLoopAsync(cancellationToken), cancellationToken);
    }

    public async Task SendTextAsync(string jid, string text, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jid);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        var payload = new
        {
            action = "send_message",
            to = jid,
            message = new { text }
        };

        var bytes = JsonSerializer.SerializeToUtf8Bytes(payload, _serializerOptions);
        await _socket.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, cancellationToken);
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[8192];

        while (_socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            var segment = new ArraySegment<byte>(buffer);
            var result = await _socket.ReceiveAsync(segment, cancellationToken);
            if (result.MessageType == WebSocketMessageType.Close)
                break;

            var payload = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var message = TryParseIncoming(payload);

            if (message is not null && MessageReceived is not null)
                await MessageReceived.Invoke(message);
        }
    }

    private WaMessage? TryParseIncoming(string payload)
    {
        try
        {
            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;

            if (!root.TryGetProperty("id", out var idElement) ||
                !root.TryGetProperty("chat", out var chatElement) ||
                !root.TryGetProperty("from", out var fromElement))
            {
                return null;
            }

            return new WaMessage(
                Id: idElement.GetString() ?? string.Empty,
                ChatJid: chatElement.GetString() ?? string.Empty,
                SenderJid: fromElement.GetString() ?? string.Empty,
                MessageType: root.TryGetProperty("type", out var typeElement) ? typeElement.GetString() ?? "unknown" : "unknown",
                Text: root.TryGetProperty("text", out var textElement) ? textElement.GetString() : null,
                TimestampUtc: DateTimeOffset.UtcNow);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_socket.State == WebSocketState.Open)
            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disposing", CancellationToken.None);

        if (_receiveLoop is not null)
            await _receiveLoop;

        _socket.Dispose();
    }
}
