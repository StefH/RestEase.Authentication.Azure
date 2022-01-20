using Azure.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestEase.Authentication.Azure.Interfaces;
using RestEase.Authentication.Azure.Options;
using Stef.Validation;

namespace RestEase.Authentication.Azure;

internal class AccessTokenService<T> : IAccessTokenService<T> where T : class
{
    private readonly ILogger<AccessTokenService<T>> _logger;
    private readonly AzureAuthenticatedRestEaseOptions<T> _options;
    private readonly IMemoryCache _cache;
    private readonly Lazy<TokenCredential> _tokenCredential;

    public AccessTokenService(
        ILogger<AccessTokenService<T>> logger,
        IOptions<AzureAuthenticatedRestEaseOptions<T>> options,
        ITokenCredentialFactory<T> factory,
        IMemoryCache cache)
    {
        _logger = Guard.NotNull(logger);
        _cache = Guard.NotNull(cache);
        _options = Guard.NotNull(options.Value);

        _tokenCredential = new Lazy<TokenCredential>(factory.CreateCredential);
    }

    public Task<string> GetTokenAsync(string resource, CancellationToken cancellationToken = default)
    {
        Guard.NotNullOrEmpty(resource);

        cancellationToken.ThrowIfCancellationRequested();

        return
            _cache.GetOrCreateAsync(GetKey(resource), async entry =>
            {
                var scopes = _options.Scopes ?? CreateScopes(resource);
                _logger.LogDebug("Getting new AccessToken for resource '{resource}' and scopes '{scopes}'.", resource, string.Join(",", scopes));

                var tokenRequestContext = new TokenRequestContext(scopes);
                var accessToken = await _tokenCredential.Value.GetTokenAsync(tokenRequestContext, cancellationToken).ConfigureAwait(false);

                entry.AbsoluteExpiration = accessToken.ExpiresOn;
                return accessToken.Token;
            });
    }

    private string GetKey(string resource)
    {
        return $"{_options.AccessTokenCacheKeyPrefix}.{resource}";
    }

    private static string[] CreateScopes(string resource)
    {
        return new[] { $"{resource}/.default" };
    }
}