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
        using var messageBuffer = new MemoryStream();

        while (_socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = await _socket.ReceiveAsync(buffer, cancellationToken);
                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                if (result.Count > 0)
                    messageBuffer.Write(buffer, 0, result.Count);

                if (!result.EndOfMessage)
                    continue;

                var payload = Encoding.UTF8.GetString(messageBuffer.GetBuffer(), 0, (int)messageBuffer.Length);
                messageBuffer.SetLength(0);

                var message = TryParseIncoming(payload);
                if (message is not null)
                    await NotifyMessageReceivedAsync(message);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (WebSocketException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
        }
    }

    private async Task NotifyMessageReceivedAsync(WaMessage message)
    {
        var handlers = MessageReceived;
        if (handlers is null)
            return;

        foreach (var handler in handlers.GetInvocationList().Cast<Func<WaMessage, ValueTask>>())
        {
            try
            {
                await handler(message);
            }
            catch
            {
                // Subscriber failures should not stop the socket loop.
            }
        }
    }

    private WaMessage? TryParseIncoming(string payload)
    {
        try
        {
            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;

            if (!TryGetString(root, "id", out var id) ||
                !TryGetString(root, "chat", out var chat) ||
                !TryGetString(root, "from", out var from))
            {
                return null;
            }

            return new WaMessage(
                Id: id,
                ChatJid: chat,
                SenderJid: from,
                MessageType: TryGetString(root, "type", out var type) ? type : "unknown",
                Text: TryGetString(root, "text", out var text) ? text : null,
                TimestampUtc: DateTimeOffset.UtcNow);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or FormatException)
        {
            return null;
        }
    }

    private static bool TryGetString(JsonElement root, string propertyName, out string value)
    {
        value = string.Empty;
        if (!root.TryGetProperty(propertyName, out var element))
            return false;
        if (element.ValueKind != JsonValueKind.String)
            return false;
        value = element.GetString() ?? string.Empty;
        return true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_socket.State == WebSocketState.Open)
            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disposing", CancellationToken.None);

        if (_receiveLoop is not null)
        {
            try
            {
                await _receiveLoop;
            }
            catch (Exception ex) when (ex is OperationCanceledException or WebSocketException or ObjectDisposedException)
            {
                // Ignore common shutdown races.
            }
        }

        _socket.Dispose();
    }
}
