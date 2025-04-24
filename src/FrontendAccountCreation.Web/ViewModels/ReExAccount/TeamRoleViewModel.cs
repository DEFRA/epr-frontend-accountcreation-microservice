using FrontendAccountCreation.Core.Sessions;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class TeamRoleViewModel
{
    [Required(ErrorMessage = "TeamRole.ErrorMessage")]
    public TeamRoleInOrganisation? TeamRoleInOrganisation { get; set; }
}