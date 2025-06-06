using FrontendAccountCreation.Web.Controllers.Attributes;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class SoleTraderTeamMemberViewModel
{
    //todo: invalid/empty - only the one error to display though
    [Required(ErrorMessage = "TeamMemberDetails.EmailError")]
    public string Email { get; set; }

    [Required(ErrorMessage = "TeamMemberDetails.TelephoneNumberEmptyErrorMessage")]
    [TeamMemberTelephoneNumberValidation(ErrorMessage = "TeamMemberDetails.TelephoneNumberInvalidErrorMessage")]
    public string Telephone { get; set; }

    [Required(ErrorMessage = "TeamMemberDetails.FirstNameErrorMessage")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "TeamMemberDetails.LastNameErrorMessage")]
    public string? LastName { get; set; }
}