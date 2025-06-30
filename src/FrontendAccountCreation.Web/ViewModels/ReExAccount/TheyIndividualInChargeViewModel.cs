using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

[ExcludeFromCodeCoverage]
public class TheyIndividualInChargeViewModel
{
    [Required(ErrorMessage = "AreTheyIndividualInCharge.ErrorMessage")]
    public YesNoAnswer? AreTheyIndividualInCharge { get; set; }
}
