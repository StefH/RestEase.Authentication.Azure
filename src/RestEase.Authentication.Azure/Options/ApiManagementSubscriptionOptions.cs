using System.ComponentModel.DataAnnotations;

namespace RestEase.Authentication.Azure.Options;

public class ApiManagementSubscriptionOptions
{
    public string? HeaderName { get; set; }

    public string? QueryParameterName { get; set; }

    [Required]
    public string Key { get; set; } = null!;
}