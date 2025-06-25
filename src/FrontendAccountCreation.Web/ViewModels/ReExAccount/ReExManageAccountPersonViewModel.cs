using FrontendAccountCreation.Core.Sessions.ReEx;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

[ExcludeFromCodeCoverage]
public class ReExManageAccountPersonViewModel
{
    [Required(ErrorMessage = "ReExManageAccountPerson.ErrorMessage.Required")]
    public ManageAccountPersonAnswer? ManageAccountPersonAnswer {  get; set; }
}
