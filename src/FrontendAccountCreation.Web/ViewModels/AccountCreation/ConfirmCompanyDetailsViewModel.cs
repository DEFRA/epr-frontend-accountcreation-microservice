using FrontendAccountCreation.Core.Addresses;

namespace FrontendAccountCreation.Web.ViewModels.AccountCreation;

public class ConfirmCompanyDetailsViewModel
{
    public string CompanyName { get; set; }

    public string CompaniesHouseNumber { get; set; }

    public Address? BusinessAddress { get; set; }
}
