using BaileysSharp.Auth;

namespace BaileysSharp.Tests;

public class AuthStateTests
{
    [Fact]
    public async Task InMemoryStore_RoundTripsCreds()
    {
        var store = new InMemoryAuthStateStore();
        var creds = AuthCreds.CreateNew();

        await store.WriteAsync(creds);
        var loaded = await store.ReadAsync();

        Assert.NotNull(loaded);
        Assert.Equal(creds.ClientId, loaded!.ClientId);
    }
}
