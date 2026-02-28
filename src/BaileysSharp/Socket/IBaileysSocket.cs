using BaileysSharp.Models;

namespace BaileysSharp.Socket;

public interface IBaileysSocket : IAsyncDisposable
{
    event Func<WaMessage, ValueTask>? MessageReceived;

    Task ConnectAsync(CancellationToken cancellationToken = default);
    Task SendTextAsync(string jid, string text, CancellationToken cancellationToken = default);
}
