using FrontendAccountCreation.Core.Sessions;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

[ExcludeFromCodeCoverage(Justification = "Get feature branch into testing")]
public class LimitedPartnershipRoleViewModel
{
    [Required(ErrorMessage = "LimitedPartnershipRole.ErrorMessage")]
    public RoleInOrganisation? LimitedPartnershipRole { get; set; }
}
