using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using FrontendAccountCreation.Web.Configs;
using Microsoft.Extensions.Options;

namespace FrontendAccountCreation.Web.ViewModels.AccountCreation;

public class FullNameViewModel : IValidatableObject
{
    public string? FirstName { get; set; }
    
    public string? LastName { get; set; }
    
    public string PostAction { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var deploymentRole = validationContext.GetService<IOptions<DeploymentRoleOptions>>();
        if (deploymentRole?.Value == null || !deploymentRole.Value.IsRegulator())
        {
            return ValidateFirstNameProducer().Union(ValidateLastNameProducer());
        }
        else
        {
            return ValidateFirstNameRegulator().Union(ValidateLastNameRegulator());
        }
    }

    private IEnumerable<ValidationResult> ValidateFirstNameProducer()
    {
        if (string.IsNullOrWhiteSpace(LastName))
        {
            yield return new ValidationResult($"FullName.LastNameErrorMessage",
                new[] { nameof(LastName) });
        }
        if (LastName?.Length > 50)
        {
            yield return new ValidationResult($"FullName.LastNameLengthErrorMessage",
                new[] { nameof(LastName) });
        }
    }
    
    private IEnumerable<ValidationResult> ValidateLastNameProducer()
    {
        if (string.IsNullOrWhiteSpace(FirstName))
        {
            yield return new ValidationResult($"FullName.FirstNameErrorMessage",
                new[] { nameof(FirstName) });
        }
        if (FirstName?.Length > 50)
        {
            yield return new ValidationResult($"FullName.FirstNameLengthErrorMessage",
                new[] { nameof(FirstName) });
        }
    }

    private IEnumerable<ValidationResult> ValidateFirstNameRegulator()
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
        else if (FirstName != null && !Regex.IsMatch(FirstName, @"^[a-z|A-Z]*$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)))
        {
            yield return new ValidationResult($"FullName.FirstNameAlphabetOnlyErrorMessage",
                new[] { nameof(FirstName) });
        }
    }

    private IEnumerable<ValidationResult> ValidateLastNameRegulator()
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