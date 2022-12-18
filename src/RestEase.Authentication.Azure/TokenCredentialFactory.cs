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
        // 1. If TenantId, ClientId, Username and Password are defined, use UsernamePasswordCredential.
        if (!string.IsNullOrEmpty(_options.TenantId) && !string.IsNullOrEmpty(_options.ClientId) && !string.IsNullOrEmpty(_options.Username) && !string.IsNullOrEmpty(_options.Password))
        {
            return new UsernamePasswordCredential(_options.Username, _options.Password, _options.TenantId, _options.ClientId);
        }

        // 2. If TenantId, ClientId and ClientSecret are defined, use ClientSecretCredential.
        if (!string.IsNullOrEmpty(_options.TenantId) && !string.IsNullOrEmpty(_options.ClientId) && !string.IsNullOrEmpty(_options.ClientSecret))
        {
            return new ClientSecretCredential(_options.TenantId, _options.ClientId, _options.ClientSecret);
        }

        // 3. Else authenticate using DefaultAzureCredential
        return new DefaultAzureCredential();
    }
}