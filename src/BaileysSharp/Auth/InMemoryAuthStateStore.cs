namespace BaileysSharp.Auth;

public sealed class InMemoryAuthStateStore : IAuthStateStore
{
    private AuthCreds? _creds;

    public ValueTask<AuthCreds?> ReadAsync(CancellationToken cancellationToken = default)
        => ValueTask.FromResult(_creds);

    public ValueTask WriteAsync(AuthCreds creds, CancellationToken cancellationToken = default)
    {
        _creds = creds;
        return ValueTask.CompletedTask;
    }
}
