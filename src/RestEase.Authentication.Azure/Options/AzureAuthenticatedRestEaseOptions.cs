using System.ComponentModel.DataAnnotations;

namespace RestEase.Authentication.Azure.Options;

// ReSharper disable once UnusedTypeParameter
public class AzureAuthenticatedRestEaseOptions<T> where T : class
{
    public string? TenantId { get; set; }

    [Required]
    public string ClientId { get; set; } = null!;

    public string? ClientSecret { get; set; }

    public string? AccessTokenCacheKeyPrefix { get; set; }

    [Required]
    public string Resource { get; set; } = null!;

    public string[]? Scopes { get; set; }

    [Required]
    public Uri BaseAddress { get; set; } = null!;

    public string? HttpClientName { get; set; }

    /// <summary>
    /// When set to 'true', no validation is done on the HTTPS Certificate when connecting to the 'BaseAddress'.
    /// </summary>
    public bool AcceptAnyServerCertificate { get; set; } = false;

    /// <summary>
    /// This timeout in seconds defines the timeout on the HttpClient which is used to call the 'BaseAddress'.
    /// Default value is 60 seconds.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int TimeoutInSeconds { get; set; } = 60;
}