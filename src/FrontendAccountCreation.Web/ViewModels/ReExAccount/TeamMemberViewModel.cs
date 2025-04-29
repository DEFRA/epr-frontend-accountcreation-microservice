using FrontendAccountCreation.Core.Sessions.ReEx;
using FrontendAccountCreation.Web.Controllers.Attributes;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class TeamMemberViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "TeamMemberDetails.FullNameError")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "TeamMemberDetails.EmailError")]
    public string Email { get; set; }

    [TeamMemberTelephoneNumberValidation(ErrorMessage = "TelephoneNumber.TelephoneNumberErrorMessage")]
    public string Telephone { get; set; }
}