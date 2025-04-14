using System.ComponentModel.DataAnnotations;

using FrontendAccountCreation.Core.Sessions;

namespace FrontendAccountCreation.Web.ViewModels.AccountCreation;

public class TeamMemberRoleInOrganisationViewModel
{
    [Required(ErrorMessage = "TeamMemberRoleInOrganisation.ErrorMessage")]
    public RoleInOrganisation? RoleInOrganisation { get; set; }
}
