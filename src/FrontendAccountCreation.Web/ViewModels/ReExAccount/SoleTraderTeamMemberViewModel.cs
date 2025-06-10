using FrontendAccountCreation.Web.Controllers.Attributes;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class SoleTraderTeamMemberViewModel
{
    [Required(ErrorMessage = "TeamMemberDetails.EmailEmptyErrorMessage")]
    [EmailAddress(ErrorMessage= "TeamMemberDetails.EmailInvalidErrorMessage")]
    public string Email { get; set; }

    [Required(ErrorMessage = "TeamMemberDetails.TelephoneNumberEmptyErrorMessage")]
    [TeamMemberTelephoneNumberValidation(ErrorMessage = "TeamMemberDetails.TelephoneNumberInvalidErrorMessage")]
    public string Telephone { get; set; }

    [Required(ErrorMessage = "TeamMemberDetails.FirstNameErrorMessage")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "TeamMemberDetails.LastNameErrorMessage")]
    public string? LastName { get; set; }
}