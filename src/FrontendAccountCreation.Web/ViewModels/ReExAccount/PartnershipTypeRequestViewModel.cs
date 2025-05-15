using FrontendAccountCreation;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web;
using FrontendAccountCreation.Web.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

[ExcludeFromCodeCoverage(Justification = "Get feature branch into testing")]
public class PartnershipTypeRequestViewModel
{
    [Required(ErrorMessage = "PartnershipType.ErrorMessage")]
    public PartnershipType? isLimitedPartnership { get; set; }
}
