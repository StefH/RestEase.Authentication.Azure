using Azure.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestEase.Authentication.Azure.Extensions;
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
        return GetTokenInternalAndStoreInCacheAsync(Guard.NotNullOrEmpty(resource), null, false, cancellationToken);
    }

    public Task<string> GetTokenAsync(string resource, string[] scopes, CancellationToken cancellationToken = default)
    {
        return GetTokenInternalAndStoreInCacheAsync(Guard.NotNullOrEmpty(resource), Guard.NotNull(scopes), false, cancellationToken);
    }

    public Task<string> GetTokenAsync(string resource, bool forceRefresh, CancellationToken cancellationToken = default)
    {
        return GetTokenInternalAndStoreInCacheAsync(Guard.NotNullOrEmpty(resource), null, forceRefresh, cancellationToken);
    }

    public Task<string> GetTokenAsync(string resource, string[] scopes, bool forceRefresh, CancellationToken cancellationToken = default)
    {
        return GetTokenInternalAndStoreInCacheAsync(Guard.NotNullOrEmpty(resource), Guard.NotNull(scopes), forceRefresh, cancellationToken);
    }

    private Task<string> GetTokenInternalAndStoreInCacheAsync(string resource, string[]? scopes, bool forceRefresh, CancellationToken cancellationToken)
    {
        Task<string> GetAccessTokenAsync(ICacheEntry entry) => GetTokenInternalAsync(resource, scopes, entry, cancellationToken);

        return forceRefresh
            ? _cache.CreateAsync(GetKey(resource), GetAccessTokenAsync)
            : _cache.GetOrCreateAsync(GetKey(resource), GetAccessTokenAsync);
    }

    private async Task<string> GetTokenInternalAsync(string resource, string[]? scopes, ICacheEntry entry, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var scopesToUse = scopes ?? _options.Scopes ?? CreateScopes(resource);
        _logger.LogDebug("Getting new AccessToken for resource '{resource}' and scopes '{scopes}'.", resource, string.Join(",", scopesToUse));

        var tokenRequestContext = new TokenRequestContext(scopesToUse);
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
        return new[] { string.Intern($"{resource}/.default") };
    }
}