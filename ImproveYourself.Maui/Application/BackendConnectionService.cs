using System.Net;
using System.Net.Http.Headers;
using ImproveYourself.Maui.Persistence;
using ImproveYourself.Maui.Resources.Strings;

namespace ImproveYourself.Maui.Application;

public sealed record BackendConnectionResult(
    bool IsConfigured,
    bool HealthOk,
    bool ReadyOk,
    bool AuthorizationOk,
    string Message);

public interface IBackendConnectionService
{
    Task<BackendConnectionResult> CheckAsync(CancellationToken cancellationToken = default);
}

public sealed class BackendConnectionService : IBackendConnectionService
{
    private readonly HttpClient _httpClient;
    private readonly ISettingsService _settingsService;

    public BackendConnectionService(HttpClient httpClient, ISettingsService settingsService)
    {
        _httpClient = httpClient;
        _settingsService = settingsService;
    }

    public async Task<BackendConnectionResult> CheckAsync(CancellationToken cancellationToken = default)
    {
        var baseUrl = _settingsService.ReadBackendBaseUrl();
        var apiKey = _settingsService.ReadBackendApiKey();

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return new BackendConnectionResult(false, false, false, false, AppStrings.BackendProvideUrl);
        }

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri)
            || (baseUri.Scheme != Uri.UriSchemeHttp && baseUri.Scheme != Uri.UriSchemeHttps))
        {
            return new BackendConnectionResult(false, false, false, false, AppStrings.BackendInvalidUrl);
        }

        try
        {
            var healthOk = await IsSuccessAsync(baseUri, "health", apiKey: null, cancellationToken);
            var readyOk = await IsSuccessAsync(baseUri, "ready", apiKey: null, cancellationToken);
            var authStatus = await GetStatusAsync(baseUri, "auth/check", apiKey, cancellationToken);
            var authorizationOk = authStatus == HttpStatusCode.OK;

            var message = (healthOk, readyOk, authorizationOk, authStatus) switch
            {
                (true, true, true, _) => AppStrings.BackendAllOk,
                (true, false, true, _) => AppStrings.BackendNoDb,
                (true, _, false, HttpStatusCode.Unauthorized) => AppStrings.BackendBadApiKey,
                (true, _, false, _) => string.Format(AppStrings.BackendBadStatusFormat, (int)authStatus),
                _ => AppStrings.BackendUnavailable,
            };

            return new BackendConnectionResult(true, healthOk, readyOk, authorizationOk, message);
        }
        catch (TaskCanceledException)
        {
            return new BackendConnectionResult(true, false, false, false, AppStrings.BackendTimeout);
        }
        catch (Exception ex)
        {
            return new BackendConnectionResult(true, false, false, false, string.Format(AppStrings.BackendConnectionFailedFormat, ex.Message));
        }
    }

    private async Task<bool> IsSuccessAsync(
        Uri baseUri,
        string path,
        string? apiKey,
        CancellationToken cancellationToken)
    {
        var status = await GetStatusAsync(baseUri, path, apiKey, cancellationToken);
        return (int)status is >= 200 and <= 299;
    }

    private async Task<HttpStatusCode> GetStatusAsync(
        Uri baseUri,
        string path,
        string? apiKey,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(baseUri, path));

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey.Trim());
            request.Headers.Add("X-Api-Key", apiKey.Trim());
        }

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return response.StatusCode;
    }
}
