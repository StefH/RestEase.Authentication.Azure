using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Options;
using RestEase.Authentication.Azure.Interfaces;
using RestEase.Authentication.Azure.Options;
using Stef.Validation;

namespace RestEase.Authentication.Azure;

// See https://github.com/Azure/azure-sdk-for-net/blob/Azure.Identity_1.6.0/sdk/identity/Azure.Identity/README.md
internal class TokenCredentialFactory<T> : ITokenCredentialFactory<T> where T : class
{
    private readonly AzureAuthenticatedRestEaseOptions<T> _options;

    private readonly Lazy<TokenCredential> _tokenCredential;

    public TokenCredentialFactory(IOptions<AzureAuthenticatedRestEaseOptions<T>> options)
    {
        _options = Guard.NotNull(options.Value);

        _tokenCredential = new Lazy<TokenCredential>(CreateChainedTokenCredential);
    }

    public TokenCredential CreateCredential()
    {
        return _tokenCredential.Value;
    }

    private TokenCredential CreateChainedTokenCredential()
    {
        var sources = new List<TokenCredential>();

        // 1. If TenantId, ClientId and ClientSecret are defined, add ClientSecretCredential as first
        if (!string.IsNullOrEmpty(_options.TenantId) && !string.IsNullOrEmpty(_options.ClientId) && !string.IsNullOrEmpty(_options.ClientSecret))
        {
            sources.Add(new ClientSecretCredential(_options.TenantId, _options.ClientId, _options.ClientSecret));
        }

        // 2. If ClientId is defined, add DefaultAzureCredential with the ManagedIdentityClientId
        if (!string.IsNullOrEmpty(_options.ClientId))
        {
            sources.Add(new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = _options.ClientId }));
        }

        // 3. Always authenticate using DefaultAzureCredential
        sources.Add(new DefaultAzureCredential());

        return new ChainedTokenCredential(sources.ToArray());
    }
}