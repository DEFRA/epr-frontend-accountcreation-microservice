using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class TeamMemberViewModel
{
    [Required(ErrorMessage = "Enter full name")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "Enter email address")]
    [EmailAddress(ErrorMessage = "Enter a valid email address")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Enter telephone number")]
    public string Telephone { get; set; }
}