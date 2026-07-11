namespace ImproveYourself.Maui.Persistence;

using ImproveYourself.Maui.Application;

public interface IAuthTokenStore
{
    Task<StoredAuthTokens?> ReadAsync();

    Task WriteAsync(StoredAuthTokens tokens);

    Task ClearAsync();
}
