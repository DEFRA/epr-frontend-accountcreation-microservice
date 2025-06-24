using FrontendAccountCreation.Core.Sessions.ReEx;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class ReExManageAccountPersonViewModel
{
    [Required(ErrorMessage = "ReExManageAccountPerson.ErrorMessage.Required")]
    public ManageAccountPersonAnswer? ManageAccountPersonAnswer {  get; set; }
}
