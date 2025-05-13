using FrontendAccountCreation.Core.Sessions;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.AccountCreation;

public class PartnershipTypeRequestViewModel
{
    [Required(ErrorMessage = "PartnershipType.ErrorMessage")]
    public PartnershipType? isLimitedPartnership { get; set; }
}
