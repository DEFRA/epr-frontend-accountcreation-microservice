using FrontendAccountCreation.Web.Controllers.Attributes;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class TeamMemberViewModel
{
    //to-do: shouldn't be required - if it's missing it should be a system error, rather than showing "The value '' is invalid." in the error summary section on the page
    [Required]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "TeamMemberDetails.EmailError")]
    public string Email { get; set; }

    [TeamMemberTelephoneNumberValidation(ErrorMessage = "TelephoneNumber.TelephoneNumberErrorMessage")]
    public string Telephone { get; set; }

    [Required(ErrorMessage = "TeamMemberDetails.FirstNameErrorMessage")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "TeamMemberDetails.LastNameErrorMessage")]
    public string? LastName { get; set; }
}