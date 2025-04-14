using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class ReExAccountFullNameViewModel : IValidatableObject
{
    public string? FirstName { get; set; }
    
    public string? LastName { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
       // var deploymentRole = validationContext.GetService<IOptions<DeploymentRoleOptions>>();
        return ValidateFirstName().Union(ValidateLastName());
    }

    private IEnumerable<ValidationResult> ValidateFirstName()
    {
        if (FirstName?.Length > 35)
        {
            yield return new ValidationResult($"FullName.FirstNameLengthErrorMessage",
                new[] { nameof(FirstName) });
        }
        if (string.IsNullOrWhiteSpace(FirstName))
        {
            yield return new ValidationResult($"FullName.FirstNameErrorMessage",
                new[] { nameof(FirstName) });
        }
        else if (!Regex.IsMatch(FirstName, @"^[a-z|A-Z]*$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)))
        {
            yield return new ValidationResult($"FullName.FirstNameAlphabetOnlyErrorMessage",
                new[] { nameof(FirstName) });
        }
    }

    private IEnumerable<ValidationResult> ValidateLastName()
    {
        if (LastName?.Length > 35)
        {
            yield return new ValidationResult($"FullName.LastNameLengthErrorMessage",
                new[] { nameof(LastName) });
        }
        if (string.IsNullOrWhiteSpace(LastName))
        {
            yield return new ValidationResult($"FullName.LastNameErrorMessage",
                new[] { nameof(LastName) });
        }
        else if (!Regex.IsMatch(LastName, @"^[a-z|A-Z]*$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250))) 
        {
            yield return new ValidationResult($"FullName.LastNameAlphabetOnlyErrorMessage",
                new[] { nameof(LastName) });
        }
    }
}