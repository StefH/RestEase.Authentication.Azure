using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using Microsoft.Extensions.Options;
using RestEase.Authentication.Azure.Interfaces;
using RestEase.Authentication.Azure.Options;

namespace RestEase.Authentication.Azure.Authentication;

internal class AuthenticationHttpMessageHandler<T>(IOptions<AzureAuthenticatedRestEaseOptions<T>> options, IAccessTokenService<T> accessTokenService) : DelegatingHandler
    where T : class
{
    private const string Scheme = "Bearer";

    private readonly AzureAuthenticatedRestEaseOptions<T> _options = options.Value;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await SetAccessTokenAsync(request, cancellationToken).ConfigureAwait(false);

        SetApiManagementSubscriptionOptions(request);

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private async Task SetAccessTokenAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var accessToken = await accessTokenService.GetTokenAsync(_options.Resource, cancellationToken).ConfigureAwait(false);
        request.Headers.Authorization = new AuthenticationHeaderValue(Scheme, accessToken);
    }

    private void SetApiManagementSubscriptionOptions(HttpRequestMessage request)
    {
        if (_options.ApiManagementSubscriptionOptions == null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(_options.ApiManagementSubscriptionOptions.HeaderName))
        {
            request.Headers.Add(_options.ApiManagementSubscriptionOptions.HeaderName, [_options.ApiManagementSubscriptionOptions.Key]);
            return;
        }

        if (!string.IsNullOrWhiteSpace(_options.ApiManagementSubscriptionOptions.QueryParameterName) && request.RequestUri != null)
        {
            var query = HttpUtility.ParseQueryString(request.RequestUri.Query);
            query[_options.ApiManagementSubscriptionOptions.QueryParameterName] = _options.ApiManagementSubscriptionOptions.Key;

            var uriBuilder = new UriBuilder(request.RequestUri)
            {
                Query = query.ToString()
            };
            request.RequestUri = uriBuilder.Uri;
        }
    }
}