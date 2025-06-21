using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using FrontendAccountCreation;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web;
using FrontendAccountCreation.Web.ViewModels;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

[ExcludeFromCodeCoverage]
public class UkRegulatorForNonUKViewModel
{
    [Required(ErrorMessage = "UkRegulator.ErrorMessage")]
    public Nation? UkRegulatorNation { get; set; }
}

