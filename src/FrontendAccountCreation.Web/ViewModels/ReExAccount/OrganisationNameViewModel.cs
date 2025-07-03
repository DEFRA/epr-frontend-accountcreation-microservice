using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class OrganisationNameViewModel
{
    [MaxLength(170, ErrorMessage = "OrganisationName.LengthErrorMessage")]
    [Required(ErrorMessage = "OrganisationName.ErrorMessage")]
    public string? OrganisationName { get; set; } 
}
