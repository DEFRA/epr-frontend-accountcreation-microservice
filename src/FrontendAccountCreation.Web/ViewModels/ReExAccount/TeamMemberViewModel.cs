using FrontendAccountCreation.Web.Controllers.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class TeamMemberViewModel
{
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