using System.ComponentModel.DataAnnotations;
using FrontendAccountCreation;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Web;
using FrontendAccountCreation.Web.ViewModels;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;
public class UkRegulatorForNonUKViewModel
{
    [Required(ErrorMessage = "UkRegulator.ErrorMessage")]
    public Nation? UkRegulatorNation { get; set; }
}

