using FrontendAccountCreation.Web.Controllers.Attributes;

namespace FrontendAccountCreation.Web.ViewModels.AccountCreation;

public class TelephoneNumberViewModel
{
    [TelephoneNumberValidation(ErrorMessage = "TelephoneNumber.TelephoneNumberErrorMessage")]
    public string? TelephoneNumber { get; set; }

    public string? EmailAddress { get; set; }
    
    public bool IsCompaniesHouseFlow { get; set; }

    public bool IsManualInputFlow { get; set; }
}