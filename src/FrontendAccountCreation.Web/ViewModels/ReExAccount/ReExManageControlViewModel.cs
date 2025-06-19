using System.ComponentModel.DataAnnotations;
using FrontendAccountCreation.Core.Sessions.ReEx;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class ReExManageControlViewModel
{
    [Required(ErrorMessage = "ReExManageControl.ErrorMessage.Required")]
    public ManageControlAnswer? ManageControlInUKAnswer { get; set; }
}