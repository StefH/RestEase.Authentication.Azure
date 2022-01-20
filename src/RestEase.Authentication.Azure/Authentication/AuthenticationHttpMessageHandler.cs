using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using RestEase.Authentication.Azure.Interfaces;
using RestEase.Authentication.Azure.Options;
using Stef.Validation;

namespace RestEase.Authentication.Azure.Authentication;

internal class AuthenticationHttpMessageHandler<T> : DelegatingHandler where T : class
{
    private const string Scheme = "Bearer";

    private readonly AzureAuthenticatedRestEaseOptions<T> _options;
    private readonly IAccessTokenService<T> _accessTokenService;

    public AuthenticationHttpMessageHandler(IOptions<AzureAuthenticatedRestEaseOptions<T>> options, IAccessTokenService<T> accessTokenService)
    {
        _options = Guard.NotNull(options.Value);
        _accessTokenService = Guard.NotNull(accessTokenService);
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var accessToken = await _accessTokenService.GetTokenAsync(_options.Resource, cancellationToken).ConfigureAwait(false);
        request.Headers.Authorization = new AuthenticationHeaderValue(Scheme, accessToken);

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}