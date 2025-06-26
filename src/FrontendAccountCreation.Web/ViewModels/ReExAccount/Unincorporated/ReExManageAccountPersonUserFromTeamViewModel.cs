using FrontendAccountCreation.Core.Sessions.ReEx;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class ReExManageAccountPersonUserFromTeamViewModel
{
    [Required(ErrorMessage = "ReExManageAccountPersonUserFromTeam.ErrorMessage.Required")]
    public ManageAccountPersonAnswer? ManageAccountPersonAnswer {  get; set; }
}
