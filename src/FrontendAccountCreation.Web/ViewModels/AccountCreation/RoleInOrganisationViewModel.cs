using System.ComponentModel.DataAnnotations;

using FrontendAccountCreation.Core.Sessions;

namespace FrontendAccountCreation.Web.ViewModels.AccountCreation;

public class RoleInOrganisationViewModel
{
    [Required(ErrorMessage = "RoleInOrganisation.ErrorMessage")]
    public RoleInOrganisation? RoleInOrganisation { get; set; }
}
