using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FrontendAccountCreation.Core.DataAnnotations;

/// <summary>
/// Specifies that a data field value must be a valid public email address in the format "user@domain.tld".
/// The build in EmailAddress attribute does not enforce the public email address format, rather it strictly follows
/// the RFC 5322 standard, which allows for more complex email formats that are not suitable for public use.
/// </summary>
/// <remarks>This attribute validates that the value of the associated property or field is a properly formatted
/// email address. It does not check for the existence of the email address or its deliverability, only its syntactic
/// correctness. The validation is case-insensitive and allows non-empty strings that match the pattern.</remarks>
//public class PublicEmailAddressAttribute : ValidationAttribute
//{
//    // A simple regex to ensure the format is user@domain.tld
//    // It checks for a local part, an @, a domain part, a dot, and a TLD.
//    private static readonly Regex Regex = new(
//        @"^[^@\s]+@[^@\s]+\.[^@\s\.]+$",
//        RegexOptions.Compiled | RegexOptions.IgnoreCase);

//    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
//    {
//        // The attribute can be used on non-string properties, so we check for that.
//        if (value is not string valueAsString)
//        {
//            // Let the [Required] attribute handle null/empty values.
//            return ValidationResult.Success;
//        }

//        if (!string.IsNullOrWhiteSpace(valueAsString) && !Regex.IsMatch(valueAsString))
//        {
//            // Use the ErrorMessage from the attribute declaration, or a default.
//            string errorMessage = ErrorMessage ?? $"The {validationContext.DisplayName} field is not a valid public email address.";
//            return new ValidationResult(errorMessage);
//        }

//        return ValidationResult.Success;
//    }
//}

public partial class PublicEmailAddressAttribute : ValidationAttribute
{
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s\.]+$", RegexOptions.IgnoreCase)]
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