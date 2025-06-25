using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

[ExcludeFromCodeCoverage]
public class ReExRoleInOrganisationViewModel
{
    [Required(ErrorMessage = "ReExRoleInOrganisation.ErrorMessage.Required")]
    [MaxLength(450, ErrorMessage = "ReExRoleInOrganisation.ErrorMessage.MaxLength")]
    public string? Role {  get; set; }
}
