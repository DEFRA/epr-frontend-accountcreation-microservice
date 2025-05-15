using FrontendAccountCreation.Core.Sessions;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.ViewModels.AccountCreation;

[ExcludeFromCodeCoverage(Justification = "Get feature branch into testing")]
public class PartnershipTypeRequestViewModel
{
    [Required(ErrorMessage = "PartnershipType.ErrorMessage")]
    public PartnershipType? isLimitedPartnership { get; set; }
}
