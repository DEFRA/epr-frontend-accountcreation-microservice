using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class ReExRoleInOrganisationViewModel
{
    [Required(ErrorMessage = "ReExRoleInOrganisation.ErrorMessage.Required")]
    [MaxLength(450, ErrorMessage = "ReExRoleInOrganisation.ErrorMessage.MaxLength")]
    public string? Role {  get; set; }
}
