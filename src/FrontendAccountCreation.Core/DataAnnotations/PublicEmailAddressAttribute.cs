using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FrontendAccountCreation.Core.DataAnnotations;

public partial class PublicEmailAddressAttribute : ValidationAttribute
{
    [GeneratedRegex(@"^[^@\s]+@([a-zA-Z0-9]([a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z]{1,}$", RegexOptions.IgnoreCase)]
    private static partial Regex PublicEmailRegex();

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string valueAsString || string.IsNullOrWhiteSpace(valueAsString))
        {
            // Let the [Required] attribute handle null/empty values.
            return ValidationResult.Success;
        }

        if (!PublicEmailRegex().IsMatch(valueAsString))
        {
            string errorMessage = ErrorMessage ?? "Enter an email address in the correct format, like name@example.com";
            return new ValidationResult(errorMessage);
        }

        return ValidationResult.Success;
    }
}