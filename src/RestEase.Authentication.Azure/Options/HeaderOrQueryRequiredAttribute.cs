using System.ComponentModel.DataAnnotations;

namespace RestEase.Authentication.Azure.Options;

public class HeaderOrQueryRequiredAttribute : ValidationAttribute
{
    private static readonly string[] MembersToValidate =
    [
        nameof(ApiManagementSubscriptionOptions.HeaderName),
        nameof(ApiManagementSubscriptionOptions.QueryParameterName)
    ];

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not ApiManagementSubscriptionOptions options)
        {
            return new ValidationResult("Null value or invalid object type for validation.");
        }

        if (!string.IsNullOrWhiteSpace(options.HeaderName) || !string.IsNullOrWhiteSpace(options.QueryParameterName))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult($"Either {nameof(ApiManagementSubscriptionOptions.HeaderName)} or {nameof(ApiManagementSubscriptionOptions.QueryParameterName)} must be provided.", MembersToValidate);
    }
}