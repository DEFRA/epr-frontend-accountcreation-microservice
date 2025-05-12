using FrontendAccountCreation.Core.Sessions;

using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.AccountCreation;

public class LimitedPartnershipRoleViewModel
{
    [Required(ErrorMessage = "LimitedPartnershipRole.ErrorMessage")]
    public LimitedPartnershipRole? LimitedPartnershipRole { get; set; }
}
