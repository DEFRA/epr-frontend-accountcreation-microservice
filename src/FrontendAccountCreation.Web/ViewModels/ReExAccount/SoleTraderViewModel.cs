using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class SoleTraderViewModel
{
    [Required(ErrorMessage = "SoleTrader.ErrorMessage")]
    public YesNoAnswer? IsIndividualInCharge { get; set; }
}