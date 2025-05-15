using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

[ExcludeFromCodeCoverage(Justification = "Get feature branch into testing")]
public class LimitedPartnershipTypeRequestViewModel : IValidatableObject
{
    public bool hasIndividualPartners { get; set; }
    public bool hasCompanyPartners { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!hasIndividualPartners && !hasCompanyPartners)
        {
            yield return new ValidationResult(
                "LimitedPartnershipType.ErrorMessage",
                new[] { nameof(hasIndividualPartners) }  // Only attach to one field to avoid duplicate messages
            );
        }
    }
}

