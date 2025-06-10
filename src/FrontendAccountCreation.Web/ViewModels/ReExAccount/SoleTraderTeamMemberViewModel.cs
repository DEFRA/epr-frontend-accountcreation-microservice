using FrontendAccountCreation.Web.Controllers.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

[ExcludeFromCodeCoverage]
public class SoleTraderTeamMemberViewModel
{
    [Required(ErrorMessage = "TeamMemberDetails.EmailEmptyErrorMessage")]
    [EmailAddress(ErrorMessage= "TeamMemberDetails.EmailInvalidErrorMessage")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "TeamMemberDetails.TelephoneNumberEmptyErrorMessage")]
    [TeamMemberTelephoneNumberValidation(ErrorMessage = "TeamMemberDetails.TelephoneNumberInvalidErrorMessage")]
    public string? Telephone { get; set; }

    //todo: test error for too long
    [Required(ErrorMessage = "TeamMemberDetails.FirstNameErrorMessage")]
    [MaxLength(50, ErrorMessage = "TeamMemberDetails.FirstNameLengthErrorMessage")]
    public string? FirstName { get; set; }

    //todo: test error for too long
    [Required(ErrorMessage = "TeamMemberDetails.LastNameErrorMessage")]
    [MaxLength(50, ErrorMessage = "TeamMemberDetails.LastNameLengthErrorMessage")]
    public string? LastName { get; set; }
}