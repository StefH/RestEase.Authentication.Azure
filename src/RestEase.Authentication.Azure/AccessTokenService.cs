using Azure.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestEase.Authentication.Azure.Extensions;
using RestEase.Authentication.Azure.Interfaces;
using RestEase.Authentication.Azure.Options;
using Stef.Validation;

namespace RestEase.Authentication.Azure;

internal class AccessTokenService<T>(
    ILogger<AccessTokenService<T>> logger,
    IOptions<AzureAuthenticatedRestEaseOptions<T>> options,
    ITokenCredentialFactory<T> factory,
    IMemoryCache cache) : IAccessTokenService<T>
    where T : class
{
    private readonly AzureAuthenticatedRestEaseOptions<T> _options = options.Value;
    private readonly Lazy<TokenCredential> _tokenCredential = new(factory.CreateCredential);

    public Task<string> GetTokenAsync(string resource, CancellationToken cancellationToken = default) =>
        GetTokenAsync(resource, false, cancellationToken);

    public Task<string> GetTokenAsync(string resource, bool forceRefresh, CancellationToken cancellationToken = default)
    {
        Guard.NotNullOrEmpty(resource);

        cancellationToken.ThrowIfCancellationRequested();

        return forceRefresh ? cache.CreateAsync(GetKey(resource), GetAccessTokenAsync) : cache.GetOrCreateAsync(GetKey(resource), GetAccessTokenAsync)!;

        Task<string> GetAccessTokenAsync(ICacheEntry entry) => GetTokenInternalAsync(resource, entry, cancellationToken);
    }

    private async Task<string> GetTokenInternalAsync(string resource, ICacheEntry entry, CancellationToken cancellationToken)
    {
        var scopes = _options.Scopes ?? CreateScopes(resource);
        logger.LogDebug("Getting new AccessToken for resource '{resource}' and scopes '{scopes}'.", resource, string.Join(",", scopes));

        var tokenRequestContext = new TokenRequestContext(scopes);
        var accessToken = await _tokenCredential.Value.GetTokenAsync(tokenRequestContext, cancellationToken).ConfigureAwait(false);

        entry.AbsoluteExpiration = accessToken.ExpiresOn;
        return accessToken.Token;
    }

    private string GetKey(string resource)
    {
        return $"{_options.AccessTokenCacheKeyPrefix}.{resource}";
    }

    private static string[] CreateScopes(string resource)
    {
        return [string.Intern($"{resource}/.default")];
    }
}