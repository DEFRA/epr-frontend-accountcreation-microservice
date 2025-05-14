using FrontendAccountCreation.Core.Sessions.ReEx;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class LimitedPartnershipApprovedPersonRoleViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "PartnershipApprovedPersonRole.ErrorMessage")]
    public ReExTeamMemberRole? RoleInOrganisation { get; set; }
}
