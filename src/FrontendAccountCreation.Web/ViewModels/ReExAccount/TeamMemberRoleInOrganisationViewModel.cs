using System.ComponentModel.DataAnnotations;
using FrontendAccountCreation.Core.Sessions.ReEx;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class TeamMemberRoleInOrganisationViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "TeamMemberRoleInOrganisation.ErrorMessage")]
    public ReExTeamMemberRole? RoleInOrganisation { get; set; }
}
