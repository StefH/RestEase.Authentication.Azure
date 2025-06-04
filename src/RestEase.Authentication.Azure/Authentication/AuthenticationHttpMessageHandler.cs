using System.Net.Http;
using System.Net.Http.Headers;
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
        var accessToken = await accessTokenService.GetTokenAsync(_options.Resource, cancellationToken).ConfigureAwait(false);
        request.Headers.Authorization = new AuthenticationHeaderValue(Scheme, accessToken);

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}