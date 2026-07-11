using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ImproveYourself.Maui.Persistence;
using ImproveYourself.Maui.Resources.Strings;

namespace ImproveYourself.Maui.Application;

public interface IAuthService
{
    bool IsLoggedIn { get; }

    string? UserEmail { get; }

    string? AccessToken { get; }

    event EventHandler? AuthStateChanged;

    Task<AuthOperationResult> RegisterAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<AuthOperationResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<bool> TryRestoreSessionAsync(CancellationToken cancellationToken = default);

    Task<bool> TryRefreshAsync(CancellationToken cancellationToken = default);

    Task LogoutAsync(CancellationToken cancellationToken = default);
}

public sealed class AuthService : IAuthService
{
    private static readonly JsonSerializerOptions ApiJsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly ISettingsService _settingsService;
    private readonly IAuthTokenStore _tokenStore;
    private readonly SemaphoreSlim _sessionLock = new(1, 1);

    private StoredAuthTokens? _currentTokens;

    public AuthService(HttpClient httpClient, ISettingsService settingsService, IAuthTokenStore tokenStore)
    {
        _httpClient = httpClient;
        _settingsService = settingsService;
        _tokenStore = tokenStore;
    }

    public bool IsLoggedIn =>
        _currentTokens is not null
        && !string.IsNullOrWhiteSpace(_currentTokens.AccessToken)
        && _currentTokens.RefreshTokenExpiresAt > DateTimeOffset.UtcNow;

    public string? UserEmail => _currentTokens?.Email;

    public string? AccessToken => _currentTokens?.AccessToken;

    public event EventHandler? AuthStateChanged;

    public async Task<AuthOperationResult> RegisterAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        return await AuthenticateAsync("api/auth/register", email, password, cancellationToken);
    }

    public async Task<AuthOperationResult> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        return await AuthenticateAsync("api/auth/login", email, password, cancellationToken);
    }

    public async Task<bool> TryRestoreSessionAsync(CancellationToken cancellationToken = default)
    {
        _currentTokens = await _tokenStore.ReadAsync();

        if (_currentTokens is null)
        {
            return false;
        }

        if (_currentTokens.AccessTokenExpiresAt > DateTimeOffset.UtcNow.AddMinutes(1))
        {
            NotifyAuthStateChanged();
            return true;
        }

        if (_currentTokens.RefreshTokenExpiresAt <= DateTimeOffset.UtcNow)
        {
            await _sessionLock.WaitAsync(cancellationToken);

            try
            {
                await ClearSessionAsync();
            }
            finally
            {
                _sessionLock.Release();
            }

            return false;
        }

        return await TryRefreshAsync(cancellationToken);
    }

    public async Task<bool> TryRefreshAsync(CancellationToken cancellationToken = default)
    {
        await _sessionLock.WaitAsync(cancellationToken);

        try
        {
            _currentTokens ??= await _tokenStore.ReadAsync();

            if (_currentTokens is not null
                && _currentTokens.AccessTokenExpiresAt > DateTimeOffset.UtcNow.AddMinutes(1))
            {
                return true;
            }

            if (_currentTokens is null || _currentTokens.RefreshTokenExpiresAt <= DateTimeOffset.UtcNow)
            {
                await ClearSessionAsync();
                return false;
            }

            var baseUri = ReadBackendBaseUri();
            if (baseUri is null)
            {
                return false;
            }

            try
            {
                using var response = await _httpClient.PostAsJsonAsync(
                    new Uri(baseUri, "api/auth/refresh"),
                    new { refreshToken = _currentTokens.RefreshToken },
                    ApiJsonOptions,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    if (ShouldClearSessionOnRefreshFailure(response.StatusCode))
                    {
                        await ClearSessionAsync();
                    }

                    return false;
                }

                var tokens = await response.Content.ReadFromJsonAsync<AuthTokensResponse>(ApiJsonOptions, cancellationToken);
                if (tokens is null)
                {
                    return false;
                }

                await PersistTokensAsync(tokens);
                return true;
            }
            catch
            {
                return false;
            }
        }
        finally
        {
            _sessionLock.Release();
        }
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        await _sessionLock.WaitAsync(cancellationToken);

        try
        {
            var baseUri = ReadBackendBaseUri();
            var refreshToken = _currentTokens?.RefreshToken;
            var accessToken = _currentTokens?.AccessToken;

            if (baseUri is not null
                && !string.IsNullOrWhiteSpace(refreshToken)
                && !string.IsNullOrWhiteSpace(accessToken))
            {
                try
                {
                    using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(baseUri, "api/auth/logout"))
                    {
                        Content = JsonContent.Create(new { refreshToken }, options: ApiJsonOptions),
                    };
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Bearer",
                        accessToken);

                    using var response = await _httpClient.SendAsync(
                        request,
                        HttpCompletionOption.ResponseHeadersRead,
                        cancellationToken);
                }
                catch
                {
                    // Logout should always clear local session.
                }
            }

            await ClearSessionAsync();
        }
        finally
        {
            _sessionLock.Release();
        }
    }

    private async Task<AuthOperationResult> AuthenticateAsync(
        string path,
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var baseUri = ReadBackendBaseUri();
        if (baseUri is null)
        {
            return new AuthOperationResult(false, AppStrings.BackendProvideUrl);
        }

        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                new Uri(baseUri, path),
                new { email, password },
                ApiJsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new AuthOperationResult(false, await ReadErrorMessageAsync(response, cancellationToken));
            }

            var tokens = await response.Content.ReadFromJsonAsync<AuthTokensResponse>(ApiJsonOptions, cancellationToken);
            if (tokens is null)
            {
                return new AuthOperationResult(false, AppStrings.AuthFailed);
            }

            await _sessionLock.WaitAsync(cancellationToken);

            try
            {
                await PersistTokensAsync(tokens);
            }
            finally
            {
                _sessionLock.Release();
            }

            return new AuthOperationResult(true, AppStrings.AuthSucceeded, tokens);
        }
        catch (TaskCanceledException)
        {
            return new AuthOperationResult(false, AppStrings.BackendTimeout);
        }
        catch (Exception ex)
        {
            return new AuthOperationResult(false, string.Format(AppStrings.BackendConnectionFailedFormat, ex.Message));
        }
    }

    private async Task PersistTokensAsync(AuthTokensResponse tokens)
    {
        _currentTokens = new StoredAuthTokens(
            tokens.AccessToken,
            tokens.RefreshToken,
            tokens.AccessTokenExpiresAt,
            tokens.RefreshTokenExpiresAt,
            tokens.UserId,
            tokens.Email);

        await _tokenStore.WriteAsync(_currentTokens);
        NotifyAuthStateChanged();
    }

    private async Task ClearSessionAsync()
    {
        _currentTokens = null;
        await _tokenStore.ClearAsync();
        NotifyAuthStateChanged();
    }

    private Uri? ReadBackendBaseUri()
    {
        var baseUrl = _settingsService.ReadBackendBaseUrl();

        if (string.IsNullOrWhiteSpace(baseUrl)
            || !Uri.TryCreate(baseUrl.Trim().TrimEnd('/'), UriKind.Absolute, out var baseUri)
            || (baseUri.Scheme != Uri.UriSchemeHttp && baseUri.Scheme != Uri.UriSchemeHttps))
        {
            return null;
        }

        return baseUri;
    }

    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>(ApiJsonOptions, cancellationToken);
            if (payload is not null && payload.TryGetValue("error", out var error) && error is not null)
            {
                return error.ToString() ?? AppStrings.AuthFailed;
            }
        }
        catch
        {
            // Fall back to generic message.
        }

        return response.StatusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized => AppStrings.AuthInvalidCredentials,
            System.Net.HttpStatusCode.Conflict => AppStrings.AuthEmailAlreadyRegistered,
            _ => AppStrings.AuthFailed,
        };
    }

    private static bool ShouldClearSessionOnRefreshFailure(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.BadRequest
            or HttpStatusCode.Unauthorized
            or HttpStatusCode.Forbidden;

    private void NotifyAuthStateChanged() => AuthStateChanged?.Invoke(this, EventArgs.Empty);
}
