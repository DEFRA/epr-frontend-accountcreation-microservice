using FrontendAccountCreation.Core.Sessions.ReEx;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

[ExcludeFromCodeCoverage]
public class ReExManageControlViewModel
{
    [Required(ErrorMessage = "ReExManageControl.ErrorMessage.Required")]
    public ManageControlAnswer? ManageControlInUKAnswer { get; set; }
}