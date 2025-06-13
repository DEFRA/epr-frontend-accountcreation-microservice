using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

[ExcludeFromCodeCoverage]
public class SoleTraderViewModel
{
    [Required(ErrorMessage = "SoleTrader.ErrorMessage")]
    public YesNoAnswer? IsIndividualInCharge { get; set; }
}