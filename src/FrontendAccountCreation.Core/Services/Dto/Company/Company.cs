using FrontendAccountCreation.Core.Services.Dto.CompaniesHouse;

namespace FrontendAccountCreation.Core.Services.Dto.Company;

public class Company
{
    public Company()
    {
    }

    public Company(CompaniesHouseCompany? organisation) : this()
    {
        if (organisation == null)
        {
            throw new ArgumentException("Organisation cannot be null.");
        }

        CompaniesHouseNumber = organisation.Organisation.RegistrationNumber ?? string.Empty;
        Name = organisation.Organisation.Name ?? string.Empty;
        BusinessAddress = new Addresses.Address(organisation.Organisation.RegisteredOffice);
        AccountCreatedOn = organisation.AccountCreatedOn;
    }

    public string Name { get; set; }

    public string CompaniesHouseNumber { get; set; }

    public Addresses.Address BusinessAddress { get; set; }

    public bool AccountExists => AccountCreatedOn is not null;

    public DateTimeOffset? AccountCreatedOn { get; set; }

}
