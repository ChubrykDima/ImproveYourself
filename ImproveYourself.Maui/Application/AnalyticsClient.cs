using System.Net.Http.Json;
using System.Text.Json;
using ImproveYourself.Maui.Persistence;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;

namespace ImproveYourself.Maui.Application;

public static class AnalyticsEventNames
{
    public const string AppOpened = "app_opened";
    public const string OnboardingCompleted = "onboarding_completed";
    public const string SelfAssessmentCompleted = "self_assessment_completed";
    public const string ChallengeOpened = "challenge_opened";
    public const string StepStarted = "step_started";
    public const string StepCompleted = "step_completed";
    public const string ChallengeCompleted = "challenge_completed";
    public const string NotificationEnabled = "notification_enabled";
    public const string BackendSyncSucceeded = "backend_sync_succeeded";
    public const string BackendSyncFailed = "backend_sync_failed";

    private static readonly HashSet<string> Allowed = new(StringComparer.Ordinal)
    {
        AppOpened,
        OnboardingCompleted,
        SelfAssessmentCompleted,
        ChallengeOpened,
        StepStarted,
        StepCompleted,
        ChallengeCompleted,
        NotificationEnabled,
        BackendSyncSucceeded,
        BackendSyncFailed,
    };

    public static bool IsAllowed(string eventName) => Allowed.Contains(eventName);
}

public interface IAnalyticsClient
{
    Task TrackAsync(
        string eventName,
        IReadOnlyDictionary<string, string>? properties = null,
        CancellationToken cancellationToken = default);
}

public sealed class AnalyticsClient : IAnalyticsClient
{
    private const int MaxPropertyValueLength = 80;

    private static readonly JsonSerializerOptions ApiJsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly HashSet<string> AllowedPropertyNames = new(StringComparer.Ordinal)
    {
        "app_version",
        "app_build",
        "platform",
        "idiom",
        "kind",
        "challenge_date",
        "challenge_status",
        "step_type",
        "step_status",
        "notifications_enabled",
        "configured",
        "has_stats",
        "failure_type",
    };

    private readonly HttpClient _httpClient;
    private readonly ISettingsService _settingsService;
    private readonly IAuthService _authService;

    public AnalyticsClient(HttpClient httpClient, ISettingsService settingsService, IAuthService authService)
    {
        _httpClient = httpClient;
        _settingsService = settingsService;
        _authService = authService;
    }

    public async Task TrackAsync(
        string eventName,
        IReadOnlyDictionary<string, string>? properties = null,
        CancellationToken cancellationToken = default)
    {
        if (!AnalyticsEventNames.IsAllowed(eventName) || !_authService.IsLoggedIn)
        {
            return;
        }

        try
        {
            var baseUri = ReadBackendBaseUri();
            if (baseUri is null)
            {
                return;
            }

            var requestBody = new AnalyticsEventRequest(
                Guid.NewGuid(),
                eventName,
                BuildEventData(properties),
                DateTimeOffset.UtcNow,
                _settingsService.ReadBackendClientId());

            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(baseUri, "api/analytics"))
            {
                Content = JsonContent.Create(requestBody, options: ApiJsonOptions),
            };

            using var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
        }
        catch
        {
            // Analytics must never affect core offline-first app behavior.
        }
    }

    private string BuildEventData(IReadOnlyDictionary<string, string>? properties)
    {
        var eventData = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["app_version"] = SafeValue(AppInfo.Current.VersionString),
            ["app_build"] = SafeValue(AppInfo.Current.BuildString),
            ["platform"] = SafeValue(DeviceInfo.Current.Platform.ToString()),
            ["idiom"] = SafeValue(DeviceInfo.Current.Idiom.ToString()),
        };

        if (properties is not null)
        {
            foreach (var (key, value) in properties)
            {
                if (!AllowedPropertyNames.Contains(key))
                {
                    continue;
                }

                eventData[key] = SafeValue(value);
            }
        }

        return JsonSerializer.Serialize(eventData, ApiJsonOptions);
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

    private static string SafeValue(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "unknown" : value.Trim();

        return normalized.Length <= MaxPropertyValueLength
            ? normalized
            : normalized[..MaxPropertyValueLength];
    }

    private sealed record AnalyticsEventRequest(
        Guid Id,
        string EventName,
        string EventData,
        DateTimeOffset Timestamp,
        string ClientId);
}
