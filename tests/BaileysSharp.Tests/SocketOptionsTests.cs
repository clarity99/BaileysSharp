using BaileysSharp.Socket;

namespace BaileysSharp.Tests;

public class SocketOptionsTests
{
    [Fact]
    public void Validate_Throws_ForNonSecureWebSocketEndpoint()
    {
        var options = new BaileysSocketOptions
        {
            WebSocketEndpoint = new Uri("ws://example.com")
        };

        Assert.Throws<ArgumentException>(() => options.Validate());
    }
}
