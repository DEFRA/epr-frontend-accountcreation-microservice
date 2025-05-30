using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class LimitedLiabilityPartnershipViewModel
{
    [Required(ErrorMessage = "LimitedLiabilityPartnership.ErrorMessage")]
    public bool? IsMemberOfLimitedLiabilityPartnership { get; set; }
}
