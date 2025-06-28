using FrontendAccountCreation.Core.DataAnnotations;
using FrontendAccountCreation.Web.Controllers.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

[ExcludeFromCodeCoverage]
public class NonCompaniesHouseTeamMemberViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "TeamMemberDetails.EmailEmptyErrorMessage")]
    [PublicEmailAddress(ErrorMessage= "TeamMemberDetails.EmailInvalidErrorMessage")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "TeamMemberDetails.TelephoneNumberEmptyErrorMessage")]
    [TeamMemberTelephoneNumberValidation(ErrorMessage = "TeamMemberDetails.TelephoneNumberInvalidErrorMessage")]
    public string? Telephone { get; set; }

    [Required(ErrorMessage = "TeamMemberDetails.FirstNameErrorMessage")]
    [MaxLength(50, ErrorMessage = "TeamMemberDetails.FirstNameLengthErrorMessage")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "TeamMemberDetails.LastNameErrorMessage")]
    [MaxLength(50, ErrorMessage = "TeamMemberDetails.LastNameLengthErrorMessage")]
    public string? LastName { get; set; }
}