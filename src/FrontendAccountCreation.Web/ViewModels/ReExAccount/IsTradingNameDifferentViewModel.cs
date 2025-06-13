using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class IsTradingNameDifferentViewModel
{
    [Required(ErrorMessage = "IsTradingNameDifferent.ErrorMessage")]
    public YesNoAnswer? IsTradingNameDifferent { get; set; }
}