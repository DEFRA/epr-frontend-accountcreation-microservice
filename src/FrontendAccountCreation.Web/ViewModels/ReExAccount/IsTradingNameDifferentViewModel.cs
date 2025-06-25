using System.ComponentModel.DataAnnotations;
using FrontendAccountCreation.Core.Sessions;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class IsTradingNameDifferentViewModel : IValidatableObject
{
    private static readonly RequiredAttribute RequiredAttribute = new();

    public YesNoAnswer? IsTradingNameDifferent { get; set; }

    public bool IsNonUk { get; set; }

    public bool IsCompaniesHouseFlow { get; set; }
    
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!RequiredAttribute.IsValid(IsTradingNameDifferent))
        {
            string errorMessageKey = IsNonUk 
                ? "IsTradingNameDifferent.NonUk.ErrorMessage"
                : "IsTradingNameDifferent.ErrorMessage";

            yield return new ValidationResult(errorMessageKey, [nameof(IsTradingNameDifferent)]);
        }
    }
}