using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class OrganisationNameViewModel
{
    [MaxLength(170, ErrorMessage = "NonUkOrganisationName.LengthErrorMessage")]
    [Required(ErrorMessage = "NonUkOrganisationName.ErrorMessage")]
    public string? OrganisationName { get; set; } 
}
