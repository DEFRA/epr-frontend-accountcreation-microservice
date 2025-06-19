using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class NonUkOrganisationNameViewModel
{
    [MaxLength(170, ErrorMessage = "NonUkOrganisationName.LengthErrorMessage")]
    [Required(ErrorMessage = "NonUkOrganisationName.ErrorMessage")]
    public string? NonUkOrganisationName { get; set; }
}
