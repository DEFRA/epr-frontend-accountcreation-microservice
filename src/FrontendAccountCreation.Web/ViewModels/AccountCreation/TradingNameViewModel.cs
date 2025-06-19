using System.ComponentModel.DataAnnotations;
using FrontendAccountCreation.Core.Sessions;

namespace FrontendAccountCreation.Web.ViewModels.AccountCreation;

public class TradingNameViewModel
{
    [MaxLength(170, ErrorMessage = "TradingName.LengthErrorMessage")]
    [Required(ErrorMessage = "TradingName.ErrorMessage")]
    public string? TradingName { get; set; }

    public ProducerType? ProducerType { get; set; }
}
