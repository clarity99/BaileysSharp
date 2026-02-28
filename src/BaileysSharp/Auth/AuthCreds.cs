namespace BaileysSharp.Auth;

public sealed record AuthCreds(
    string ClientId,
    string? DeviceId,
    byte[] NoiseKey,
    byte[] SignedIdentityKey,
    DateTimeOffset CreatedAtUtc)
{
    public static AuthCreds CreateNew() => new(
        ClientId: Guid.NewGuid().ToString("N"),
        DeviceId: null,
        NoiseKey: RandomNumberGenerator.GetBytes(32),
        SignedIdentityKey: RandomNumberGenerator.GetBytes(32),
        CreatedAtUtc: DateTimeOffset.UtcNow);
}
