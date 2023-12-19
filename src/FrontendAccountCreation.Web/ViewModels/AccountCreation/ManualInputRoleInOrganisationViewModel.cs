using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.AccountCreation;

public class ManualInputRoleInOrganisationViewModel
{
    [MaxLength(450, ErrorMessage = "ManualInputRoleInOrganisation.LengthErrorMessage")]
    [Required(ErrorMessage = "ManualInputRoleInOrganisation.ErrorMessage")]
    public string? RoleInOrganisation { get; set; }
}
