using ImproveYourself.Maui.Application;

namespace ImproveYourself.Maui.Persistence;

public sealed class SecureAuthTokenStore : IAuthTokenStore
{
    private const string AccessTokenKey = "auth.accessToken";
    private const string RefreshTokenKey = "auth.refreshToken";
    private const string AccessExpiresKey = "auth.accessExpiresAt";
    private const string RefreshExpiresKey = "auth.refreshExpiresAt";
    private const string UserIdKey = "auth.userId";
    private const string EmailKey = "auth.email";

    public async Task<StoredAuthTokens?> ReadAsync()
    {
        try
        {
            var accessToken = await SecureStorage.Default.GetAsync(AccessTokenKey);
            var refreshToken = await SecureStorage.Default.GetAsync(RefreshTokenKey);
            var accessExpiresRaw = await SecureStorage.Default.GetAsync(AccessExpiresKey);
            var refreshExpiresRaw = await SecureStorage.Default.GetAsync(RefreshExpiresKey);
            var userIdRaw = await SecureStorage.Default.GetAsync(UserIdKey);
            var email = await SecureStorage.Default.GetAsync(EmailKey);

            if (string.IsNullOrWhiteSpace(accessToken)
                || string.IsNullOrWhiteSpace(refreshToken)
                || string.IsNullOrWhiteSpace(accessExpiresRaw)
                || string.IsNullOrWhiteSpace(refreshExpiresRaw)
                || string.IsNullOrWhiteSpace(userIdRaw)
                || string.IsNullOrWhiteSpace(email)
                || !DateTimeOffset.TryParse(accessExpiresRaw, out var accessExpiresAt)
                || !DateTimeOffset.TryParse(refreshExpiresRaw, out var refreshExpiresAt)
                || !Guid.TryParse(userIdRaw, out var userId))
            {
                return null;
            }

            return new StoredAuthTokens(
                accessToken,
                refreshToken,
                accessExpiresAt,
                refreshExpiresAt,
                userId,
                email);
        }
        catch
        {
            return null;
        }
    }

    public async Task WriteAsync(StoredAuthTokens tokens)
    {
        await SecureStorage.Default.SetAsync(AccessTokenKey, tokens.AccessToken);
        await SecureStorage.Default.SetAsync(RefreshTokenKey, tokens.RefreshToken);
        await SecureStorage.Default.SetAsync(AccessExpiresKey, tokens.AccessTokenExpiresAt.ToString("O"));
        await SecureStorage.Default.SetAsync(RefreshExpiresKey, tokens.RefreshTokenExpiresAt.ToString("O"));
        await SecureStorage.Default.SetAsync(UserIdKey, tokens.UserId.ToString("D"));
        await SecureStorage.Default.SetAsync(EmailKey, tokens.Email);
    }

    public Task ClearAsync()
    {
        SecureStorage.Default.Remove(AccessTokenKey);
        SecureStorage.Default.Remove(RefreshTokenKey);
        SecureStorage.Default.Remove(AccessExpiresKey);
        SecureStorage.Default.Remove(RefreshExpiresKey);
        SecureStorage.Default.Remove(UserIdKey);
        SecureStorage.Default.Remove(EmailKey);

        return Task.CompletedTask;
    }
}
