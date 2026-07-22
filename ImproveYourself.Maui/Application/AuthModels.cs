namespace ImproveYourself.Maui.Application;

public sealed record StoredAuthTokens(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    DateTimeOffset RefreshTokenExpiresAt,
    Guid UserId,
    string Email);

public sealed record AuthTokensResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    DateTimeOffset RefreshTokenExpiresAt,
    Guid UserId,
    string Email);

public sealed record AuthOperationResult(
    bool Succeeded,
    string Message,
    AuthTokensResponse? Tokens = null,
    bool BackendEndpointMissing = false);
