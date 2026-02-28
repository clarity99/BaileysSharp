namespace BaileysSharp.Auth;

public sealed class InMemoryAuthStateStore : IAuthStateStore
{
    private AuthCreds? _creds;

    public ValueTask<AuthCreds?> ReadAsync(CancellationToken cancellationToken = default)
        => ValueTask.FromResult(_creds?.DeepCopy());

    public ValueTask WriteAsync(AuthCreds creds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(creds);
        _creds = creds.DeepCopy();
        return ValueTask.CompletedTask;
    }
}
