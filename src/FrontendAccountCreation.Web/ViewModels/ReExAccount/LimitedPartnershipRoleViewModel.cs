using FrontendAccountCreation.Core.Sessions;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class LimitedPartnershipRoleViewModel
{
    [Required(ErrorMessage = "LimitedPartnershipRole.ErrorMessage")]
    public RoleInOrganisation? RoleInOrganisation { get; set; }
}
