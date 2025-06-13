using FrontendAccountCreation.Web.Controllers.Attributes;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class ReExAccountTelephoneNumberViewModel
{
    [TelephoneNumberValidation(ErrorMessage = "TelephoneNumber.TelephoneNumberErrorMessage")]
    public string? TelephoneNumber { get; set; }

    public string? EmailAddress { get; set; }
}