using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.AccountCreation;

public class RegisteredAsCharityRequestViewModel
{
    [Required(ErrorMessage = "RegisteredAsCharity.ErrorMessage")]
    public YesNoAnswer? isTheOrganisationCharity { get; set; }
}
