using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using FrontendAccountCreation.Core.DataAnnotations;
using FrontendAccountCreation.Web.Controllers.Attributes;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

[ExcludeFromCodeCoverage]
public class TeamMemberViewModel
{
    //to-do: shouldn't be required - if it's missing it should be a system error, rather than showing "The value '' is invalid." in the error summary section on the page
    [Required]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "TeamMemberDetails.EmailError")]
    [PublicEmailAddress(ErrorMessage = "TeamMemberDetails.EmailInvalidErrorMessage")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "TeamMemberDetails.TelephoneNumberErrorMessage")]
    [TeamMemberTelephoneNumberValidation(ErrorMessage = "TeamMemberDetails.TelephoneNumberInvalidErrorMessage")]
    public string? Telephone { get; set; }

    [Required(ErrorMessage = "TeamMemberDetails.FirstNameErrorMessage")]
    [MaxLength(50, ErrorMessage = "TeamMemberDetails.FirstNameLengthErrorMessage")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "TeamMemberDetails.LastNameErrorMessage")]
    [MaxLength(50, ErrorMessage = "TeamMemberDetails.LastNameLengthErrorMessage")]
    public string? LastName { get; set; }    
}