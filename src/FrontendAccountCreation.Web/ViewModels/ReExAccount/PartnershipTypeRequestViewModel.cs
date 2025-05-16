using FrontendAccountCreation;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web;
using FrontendAccountCreation.Web.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class PartnershipTypeRequestViewModel
{
    [Required(ErrorMessage = "PartnershipType.ErrorMessage")]
    public PartnershipType? isLimitedPartnership { get; set; }
}
