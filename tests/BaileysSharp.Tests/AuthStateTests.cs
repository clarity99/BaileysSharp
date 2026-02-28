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

    [Fact]
    public async Task InMemoryStore_WriteStoresDefensiveCopy()
    {
        var store = new InMemoryAuthStateStore();
        var noise = new byte[] { 1, 2, 3 };
        var identity = new byte[] { 4, 5, 6 };
        var creds = new AuthCreds("client", null, noise, identity, DateTimeOffset.UtcNow);

        await store.WriteAsync(creds);
        noise[0] = 99;
        identity[0] = 99;

        var loaded = await store.ReadAsync();
        Assert.Equal(new byte[] { 1, 2, 3 }, loaded!.NoiseKey.ToArray());
        Assert.Equal(new byte[] { 4, 5, 6 }, loaded.SignedIdentityKey.ToArray());
    }
}
