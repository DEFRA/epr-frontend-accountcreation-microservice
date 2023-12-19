using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.AccountCreation;

public class TradingNameViewModel
{
    [MaxLength(170, ErrorMessage = "TradingName.LengthErrorMessage")]
    [Required(ErrorMessage = "TradingName.ErrorMessage")]
    public string? TradingName { get; set; }
}
