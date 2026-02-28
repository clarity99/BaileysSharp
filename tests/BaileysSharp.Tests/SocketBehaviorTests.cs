using System.Reflection;
using BaileysSharp.Auth;
using BaileysSharp.Models;
using BaileysSharp.Socket;

namespace BaileysSharp.Tests;

public class SocketBehaviorTests
{
    [Fact]
    public void TryParseIncoming_ReturnsNull_ForNonStringId()
    {
        var socket = new BaileysSocket(new InMemoryAuthStateStore());
        var method = typeof(BaileysSocket).GetMethod("TryParseIncoming", BindingFlags.NonPublic | BindingFlags.Instance);

        var result = method!.Invoke(socket, new object[] { "{\"id\":123,\"chat\":\"a\",\"from\":\"b\"}" });
        Assert.Null(result);
    }

    [Fact]
    public async Task NotifyMessageReceivedAsync_ContinuesWhenSubscriberThrows()
    {
        var socket = new BaileysSocket(new InMemoryAuthStateStore());
        var method = typeof(BaileysSocket).GetMethod("NotifyMessageReceivedAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        var called = false;

        socket.MessageReceived += _ => throw new InvalidOperationException("fail");
        socket.MessageReceived += _ =>
        {
            called = true;
            return ValueTask.CompletedTask;
        };

        var message = new WaMessage("1", "chat", "from", "text", "hello", DateTimeOffset.UtcNow);
        var task = (Task)method!.Invoke(socket, new object[] { message })!;
        await task;

        Assert.True(called);
    }
}
