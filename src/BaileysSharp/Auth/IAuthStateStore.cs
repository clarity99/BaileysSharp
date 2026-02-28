namespace BaileysSharp.Auth;

public interface IAuthStateStore
{
    ValueTask<AuthCreds?> ReadAsync(CancellationToken cancellationToken = default);
    ValueTask WriteAsync(AuthCreds creds, CancellationToken cancellationToken = default);
}
