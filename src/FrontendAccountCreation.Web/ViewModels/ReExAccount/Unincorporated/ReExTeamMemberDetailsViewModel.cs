using FrontendAccountCreation.Web.Controllers.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount.Unincorporated;

[ExcludeFromCodeCoverage]
public class ReExTeamMemberDetailsViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "ReExTeamMemberDetails.ErrorMessage.Required.FirstName")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "ReExTeamMemberDetails.ErrorMessage.Required.LastName")]
    public string? LastName { get; set; }

    [Required(ErrorMessage = "ReExTeamMemberDetails.ErrorMessage.Required.Email")]
    [EmailAddress(ErrorMessage = "ReExTeamMemberDetails.ErrorMessage.EmailAddress")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "ReExTeamMemberDetails.ErrorMessage.Required.Telephone")]
    [TeamMemberTelephoneNumberValidation(ErrorMessage = "ReExTeamMemberDetails.ErrorMessage.TeamMemberTelephoneNumber")]
    public string? Telephone { get; set; }
}
