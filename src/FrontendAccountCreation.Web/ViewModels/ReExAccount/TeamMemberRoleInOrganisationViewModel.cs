using System.ComponentModel.DataAnnotations;
using FrontendAccountCreation;
using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web;
using FrontendAccountCreation.Web.ViewModels;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class TeamMemberRoleInOrganisationViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "TeamMemberRoleInOrganisation.ErrorMessage")]
    public ReExTeamMemberRole? RoleInOrganisation { get; set; }
}
