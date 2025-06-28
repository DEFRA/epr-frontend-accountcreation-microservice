using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class NonUkRoleInOrganisationViewModel
{
    [Required(ErrorMessage = "NonUkRoleInOrganisation.ErrorMessage")]
    public string? NonUkRoleInOrganisation { get; set; }
}
