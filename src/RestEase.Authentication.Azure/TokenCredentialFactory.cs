using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Options;
using RestEase.Authentication.Azure.Interfaces;
using RestEase.Authentication.Azure.Options;
using Stef.Validation;

// See https://azuresdkdocs.blob.core.windows.net/$web/dotnet/Azure.Identity/1.0.0/api/index.html
namespace RestEase.Authentication.Azure;

internal class TokenCredentialFactory<T> : ITokenCredentialFactory<T> where T : class
{
    private readonly AzureAuthenticatedRestEaseOptions<T> _options;

    private readonly Lazy<TokenCredential> _tokenCredential;

    public TokenCredentialFactory(IOptions<AzureAuthenticatedRestEaseOptions<T>> options)
    {
        _options = Guard.NotNull(options.Value);

        _tokenCredential = new Lazy<TokenCredential>(CreateCredentialInternal);
    }

    public TokenCredential CreateCredential()
    {
        return _tokenCredential.Value;
    }

    private TokenCredential CreateCredentialInternal()
    {
        var sources = new List<TokenCredential>();

        // 1. If TenantId, ClientId and ClientSecret are defined, add ClientSecretCredential as first
        if (!string.IsNullOrEmpty(_options.TenantId) && !string.IsNullOrEmpty(_options.ClientId) && !string.IsNullOrEmpty(_options.ClientSecret))
        {
            sources.Add(new ClientSecretCredential(_options.TenantId, _options.ClientId, _options.ClientSecret));
        }

        // 2. Authenticate using ManagedIdentityCredential
        sources.Add(new ManagedIdentityCredential(_options.ClientId));

        return new ChainedTokenCredential(sources.ToArray());
    }
}