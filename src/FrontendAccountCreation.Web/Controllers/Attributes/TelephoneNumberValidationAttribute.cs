using System.ComponentModel.DataAnnotations;
using FrontendAccountCreation.Core.Validations;

namespace FrontendAccountCreation.Web.Controllers.Attributes;

public class TelephoneNumberValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var phoneNumber = value?.ToString() ?? string.Empty;

        return TelephoneNumberValidator.IsValid(phoneNumber) ? ValidationResult.Success : new ValidationResult(ErrorMessage);
    }
}