using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class IsUkMainAddressViewModel
{
    [Required(ErrorMessage = "IsUkMainAddress.ErrorMessage")]
    public YesNoAnswer? IsUkMainAddress { get; set; }
}