using System.Net;
using System.Net.Http.Headers;

namespace ImproveYourself.Maui.Application;

public sealed class AuthenticatedHttpMessageHandler : DelegatingHandler
{
    private readonly IAuthService _authService;

    public AuthenticatedHttpMessageHandler(IAuthService authService)
    {
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        await AttachAccessTokenAsync(request);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            return response;
        }

        // No local session / no bearer was sent — do not attempt refresh.
        if (string.IsNullOrWhiteSpace(_authService.AccessToken))
        {
            return response;
        }

        response.Dispose();

        if (!await _authService.TryRefreshAsync(cancellationToken))
        {
            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }

        var retryRequest = await CloneRequestAsync(request);
        await AttachAccessTokenAsync(retryRequest);

        return await base.SendAsync(retryRequest, cancellationToken);
    }

    private Task AttachAccessTokenAsync(HttpRequestMessage request)
    {
        var accessToken = _authService.AccessToken;

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization = null;
            return Task.CompletedTask;
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return Task.CompletedTask;
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        if (request.Content is not null)
        {
            var contentBytes = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(contentBytes);

            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        foreach (var header in request.Headers)
        {
            if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}
