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

    Task<AuthOperationResult> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);

    Task<AuthOperationResult> ConfirmPasswordResetAsync(
        string email,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default);

    Task<AccountExportResult> ExportAccountAsync(CancellationToken cancellationToken = default);

    Task<AuthOperationResult> DeleteAccountAsync(CancellationToken cancellationToken = default);

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

    /// <summary>
    /// Requests a password-reset email.
    /// Backend: POST /api/auth/password-reset/request
    /// </summary>
    public async Task<AuthOperationResult> RequestPasswordResetAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = (email ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return new AuthOperationResult(false, AppStrings.AuthEmailRequired);
        }

        var baseUri = ReadBackendBaseUri();
        if (baseUri is null)
        {
            return new AuthOperationResult(false, AppStrings.BackendProvideUrl);
        }

        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                new Uri(baseUri, "api/auth/password-reset/request"),
                new { email = normalizedEmail },
                ApiJsonOptions,
                cancellationToken);

            if (IsMissingEndpointStatus(response.StatusCode))
            {
                return new AuthOperationResult(
                    false,
                    AppStrings.AuthForgotPasswordBackendMissing,
                    BackendEndpointMissing: true);
            }

            if (response.IsSuccessStatusCode)
            {
                var payload = await response.Content.ReadFromJsonAsync<PasswordResetRequestResponse>(
                    ApiJsonOptions,
                    cancellationToken);

                return new AuthOperationResult(
                    true,
                    AppStrings.AuthForgotPasswordSent,
                    ResetToken: payload?.ResetToken);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return new AuthOperationResult(false, AppStrings.AuthForgotPasswordUnavailable);
            }

            return new AuthOperationResult(false, await ReadErrorMessageAsync(response, cancellationToken));
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

    /// <summary>
    /// Confirms password reset with the emailed one-time token.
    /// Backend: POST /api/auth/password-reset/confirm
    /// </summary>
    public async Task<AuthOperationResult> ConfirmPasswordResetAsync(
        string email,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = (email ?? string.Empty).Trim();
        var normalizedToken = (token ?? string.Empty).Trim();
        var password = newPassword ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return new AuthOperationResult(false, AppStrings.AuthEmailRequired);
        }

        if (string.IsNullOrWhiteSpace(normalizedToken))
        {
            return new AuthOperationResult(false, AppStrings.AuthResetTokenRequired);
        }

        if (password.Length < 8)
        {
            return new AuthOperationResult(false, AppStrings.AuthPasswordTooShort);
        }

        var baseUri = ReadBackendBaseUri();
        if (baseUri is null)
        {
            return new AuthOperationResult(false, AppStrings.BackendProvideUrl);
        }

        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                new Uri(baseUri, "api/auth/password-reset/confirm"),
                new { email = normalizedEmail, token = normalizedToken, newPassword = password },
                ApiJsonOptions,
                cancellationToken);

            if (IsMissingEndpointStatus(response.StatusCode))
            {
                return new AuthOperationResult(
                    false,
                    AppStrings.AuthForgotPasswordBackendMissing,
                    BackendEndpointMissing: true);
            }

            if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NoContent)
            {
                return new AuthOperationResult(true, AppStrings.AuthResetPasswordSucceeded);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return new AuthOperationResult(false, AppStrings.AuthResetTokenInvalid);
            }

            return new AuthOperationResult(false, await ReadErrorMessageAsync(response, cancellationToken));
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

    /// <summary>
    /// Downloads account data JSON (profile, challenges, analytics).
    /// Backend: GET /api/auth/export — refreshes and retries once on 401.
    /// </summary>
    public async Task<AccountExportResult> ExportAccountAsync(CancellationToken cancellationToken = default)
    {
        if (!IsLoggedIn)
        {
            return new AccountExportResult(false, AppStrings.AuthLoginRequired);
        }

        var baseUri = ReadBackendBaseUri();
        if (baseUri is null)
        {
            return new AccountExportResult(false, AppStrings.BackendProvideUrl);
        }

        try
        {
            var response = await SendAuthenticatedAsync(HttpMethod.Get, baseUri, "api/auth/export", cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                response.Dispose();

                if (!await TryRefreshAsync(cancellationToken))
                {
                    await ForceClearSessionAsync(cancellationToken);
                    return new AccountExportResult(false, AppStrings.AuthSessionExpired);
                }

                response = await SendAuthenticatedAsync(HttpMethod.Get, baseUri, "api/auth/export", cancellationToken);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    response.Dispose();
                    await ForceClearSessionAsync(cancellationToken);
                    return new AccountExportResult(false, AppStrings.AuthSessionExpired);
                }
            }

            using (response)
            {
                if (IsMissingEndpointStatus(response.StatusCode))
                {
                    return new AccountExportResult(
                        false,
                        AppStrings.AuthExportBackendMissing,
                        BackendEndpointMissing: true);
                }

                if (!response.IsSuccessStatusCode)
                {
                    return new AccountExportResult(false, await ReadErrorMessageAsync(response, cancellationToken));
                }

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return new AccountExportResult(true, AppStrings.AuthExportSucceeded, json);
            }
        }
        catch (TaskCanceledException)
        {
            return new AccountExportResult(false, AppStrings.BackendTimeout);
        }
        catch (Exception ex)
        {
            return new AccountExportResult(false, string.Format(AppStrings.BackendConnectionFailedFormat, ex.Message));
        }
    }

    /// <summary>
    /// Deletes the signed-in account on the server, then clears the local session.
    /// Backend: DELETE /api/auth/account — refreshes and retries once on 401.
    /// </summary>
    public async Task<AuthOperationResult> DeleteAccountAsync(CancellationToken cancellationToken = default)
    {
        if (!IsLoggedIn)
        {
            return new AuthOperationResult(false, AppStrings.AuthLoginRequired);
        }

        var baseUri = ReadBackendBaseUri();
        if (baseUri is null)
        {
            return new AuthOperationResult(false, AppStrings.BackendProvideUrl);
        }

        try
        {
            var response = await SendAuthenticatedAsync(HttpMethod.Delete, baseUri, "api/auth/account", cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                response.Dispose();

                if (!await TryRefreshAsync(cancellationToken))
                {
                    // Refresh may fail without clearing (e.g. transient network). Delete already got 401,
                    // so drop the local session to match AuthSessionExpired.
                    await ForceClearSessionAsync(cancellationToken);
                    return new AuthOperationResult(false, AppStrings.AuthSessionExpired);
                }

                response = await SendAuthenticatedAsync(HttpMethod.Delete, baseUri, "api/auth/account", cancellationToken);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    response.Dispose();
                    await ForceClearSessionAsync(cancellationToken);
                    return new AuthOperationResult(false, AppStrings.AuthSessionExpired);
                }
            }

            using (response)
            {
                if (IsMissingEndpointStatus(response.StatusCode))
                {
                    return new AuthOperationResult(
                        false,
                        AppStrings.AuthDeleteAccountBackendMissing,
                        BackendEndpointMissing: true);
                }

                if (!response.IsSuccessStatusCode)
                {
                    return new AuthOperationResult(false, await ReadErrorMessageAsync(response, cancellationToken));
                }
            }

            await ForceClearSessionAsync(cancellationToken);
            return new AuthOperationResult(true, AppStrings.AuthDeleteAccountSucceeded);
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

    private async Task<HttpResponseMessage> SendAuthenticatedAsync(
        HttpMethod method,
        Uri baseUri,
        string relativePath,
        CancellationToken cancellationToken)
    {
        var accessToken = AccessToken;
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }

        using var request = new HttpRequestMessage(method, new Uri(baseUri, relativePath));
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer",
            accessToken);

        return await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
    }

    private async Task ForceClearSessionAsync(CancellationToken cancellationToken)
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
    }

    private static bool IsMissingEndpointStatus(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.NotFound
            or HttpStatusCode.MethodNotAllowed
            or HttpStatusCode.NotImplemented;

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
