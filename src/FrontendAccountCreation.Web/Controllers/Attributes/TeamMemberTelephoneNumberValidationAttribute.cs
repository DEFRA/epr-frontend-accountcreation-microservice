using FrontendAccountCreation.Core.Validations;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.Controllers.Attributes;

public class TeamMemberTelephoneNumberValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var phoneNumber = value?.ToString();

        // If empty, skip format validation — let [Required] handle it
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return ValidationResult.Success;
        }

        return TelephoneNumberValidator.IsValid(phoneNumber)
            ? ValidationResult.Success
            : new ValidationResult(ErrorMessage);
    }
}