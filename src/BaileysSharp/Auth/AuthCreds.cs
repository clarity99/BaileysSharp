namespace BaileysSharp.Auth;

public sealed record AuthCreds
{
    public string ClientId { get; }
    public string? DeviceId { get; }
    public ReadOnlyMemory<byte> NoiseKey { get; }
    public ReadOnlyMemory<byte> SignedIdentityKey { get; }
    public DateTimeOffset CreatedAtUtc { get; }

    public AuthCreds(
        string clientId,
        string? deviceId,
        ReadOnlyMemory<byte> noiseKey,
        ReadOnlyMemory<byte> signedIdentityKey,
        DateTimeOffset createdAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);
        ClientId = clientId;
        DeviceId = deviceId;
        NoiseKey = noiseKey.ToArray();
        SignedIdentityKey = signedIdentityKey.ToArray();
        CreatedAtUtc = createdAtUtc;
    }

    public static AuthCreds CreateNew() => new(
        clientId: Guid.NewGuid().ToString("N"),
        deviceId: null,
        noiseKey: RandomNumberGenerator.GetBytes(32),
        signedIdentityKey: RandomNumberGenerator.GetBytes(32),
        createdAtUtc: DateTimeOffset.UtcNow);

    public AuthCreds DeepCopy() => new(
        clientId: ClientId,
        deviceId: DeviceId,
        noiseKey: NoiseKey,
        signedIdentityKey: SignedIdentityKey,
        createdAtUtc: CreatedAtUtc);
}
