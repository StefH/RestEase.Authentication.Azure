using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestEase.Authentication.Azure.Interfaces;
using RestEase.Authentication.Azure.Options;

namespace RestEase.Authentication.Azure;

// See https://github.com/Azure/azure-sdk-for-net/blob/Azure.Identity_1.6.0/sdk/identity/Azure.Identity/README.md
internal class TokenCredentialFactory<T> : ITokenCredentialFactory<T> where T : class
{
    private readonly AzureAuthenticatedRestEaseOptions<T> _options;
    private readonly ILogger<TokenCredentialFactory<T>> _logger;
    private readonly Lazy<TokenCredential> _tokenCredential;

    public TokenCredentialFactory(ILogger<TokenCredentialFactory<T>> logger, IOptions<AzureAuthenticatedRestEaseOptions<T>> options)
    {
        _logger = logger;
        _options = options.Value;
        _tokenCredential = new Lazy<TokenCredential>(BuildTokenCredential);
    }

    public TokenCredential CreateCredential()
    {
        return _tokenCredential.Value;
    }

    private TokenCredential BuildTokenCredential()
    {
        // 1. If TenantId, ClientId, Username and Password are defined, use UsernamePasswordCredential
        if (!string.IsNullOrEmpty(_options.TenantId) && !string.IsNullOrEmpty(_options.ClientId) && !string.IsNullOrEmpty(_options.Username) && !string.IsNullOrEmpty(_options.Password))
        {
            _logger.LogInformation("Using TokenCredential '{TokenCredential}' for ClientId '{ClientId}'", nameof(UsernamePasswordCredential), _options.ClientId);
            return new UsernamePasswordCredential(_options.Username, _options.Password, _options.TenantId, _options.ClientId);
        }

        // 2. If TenantId, ClientId and ClientSecret are defined, use ClientSecretCredential
        if (!string.IsNullOrEmpty(_options.TenantId) && !string.IsNullOrEmpty(_options.ClientId) && !string.IsNullOrEmpty(_options.ClientSecret))
        {
            _logger.LogInformation("Using TokenCredential '{TokenCredential}' for ClientId '{ClientId}'", nameof(ClientSecretCredential), _options.ClientId);
            return new ClientSecretCredential(_options.TenantId, _options.ClientId, _options.ClientSecret);
        }

        // 3. If ClientId is defined, use DefaultAzureCredential with ManagedIdentityClientId
        // - https://learn.microsoft.com/en-us/dotnet/api/azure.identity.defaultazurecredential?view=azure-dotnet#examples
        // - https://github.com/Azure/azure-sdk-for-net/issues/35128
        if (!string.IsNullOrEmpty(_options.ClientId))
        {
            _logger.LogInformation("Using TokenCredential '{TokenCredential}' for ManagedIdentityClientId '{ManagedIdentityClientId}'", nameof(DefaultAzureCredential), _options.ClientId);
            return new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ManagedIdentityClientId = _options.ClientId
            });
        }

        // 4. Default option, use DefaultAzureCredential
        _logger.LogInformation("Using TokenCredential '{TokenCredential}'", nameof(DefaultAzureCredential));
        return new DefaultAzureCredential();
    }
}