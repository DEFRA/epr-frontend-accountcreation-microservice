using FrontendAccountCreation.Core.Services.FacadeModels;
using FrontendAccountCreation.Core.Sessions;

namespace FrontendAccountCreation.Core.Services;

public class AccountMapper : IAccountMapper
{
    private const string ApprovedPersonServiceRole = "Packaging.ApprovedPerson";
    
    public AccountModel CreateAccountModel(AccountCreationSession session, string email)
    {
        bool isComplianceScheme;
        string companiesHouseNumber;
        bool validatedWithCompaniesHouse;
        string name;
        string? jobTitle;
        string organisationId;
        ProducerType? producerType;

        if (session.IsCompaniesHouseFlow)
        {
            name = session.CompaniesHouseSession.Company.Name;
            companiesHouseNumber = session.CompaniesHouseSession.Company.CompaniesHouseNumber;
            isComplianceScheme = session.CompaniesHouseSession.IsComplianceScheme;
            validatedWithCompaniesHouse = true;
            jobTitle = session.CompaniesHouseSession.RoleInOrganisation.ToString();
            producerType = null;
            organisationId = session.CompaniesHouseSession.Company.OrganisationId;
        }
        else
        {
            name = session.ManualInputSession.TradingName;
            companiesHouseNumber = null;
            isComplianceScheme = false;
            validatedWithCompaniesHouse = false;
            jobTitle = session.ManualInputSession.RoleInOrganisation;
            producerType = session.ManualInputSession.ProducerType;
            organisationId = session.ManualInputSession.OrganisationId;
        }

        var person = new PersonModel()
        {
            FirstName = session.Contact.FirstName,
            LastName = session.Contact.LastName,
            ContactEmail = email,
            TelephoneNumber = session.Contact.TelephoneNumber
        };

        var organisation = new OrganisationModel()
        {
            ProducerType = producerType,
            OrganisationType = session.OrganisationType,
            CompaniesHouseNumber = companiesHouseNumber,
            Name = name,
            Address = GetCompanyAddress(session),
            ValidatedWithCompaniesHouse = validatedWithCompaniesHouse,
            IsComplianceScheme = isComplianceScheme,
            Nation = session.UkNation ?? Nation.NotSet,
            OrganisationId = organisationId
        };

        var account = new AccountModel()
        {
            Person = person,
            Organisation = organisation,
            Connection = new ConnectionModel()
            {
                ServiceRole = ApprovedPersonServiceRole,
                JobTitle = jobTitle,
            }
        };

        if (session.IsApprovedUser)
        {
            account.DeclarationTimeStamp = session.DeclarationTimestamp;
            account.DeclarationFullName = session.DeclarationFullName;
        }

        return account;
    }

    //todo: is it appropriate to say we're creating an account, or do we just want to create a person/user?
    //public AccountModel CreateReExAccountModel(AccountCreationSession session, string email)
    // if keep with person, move out of account mapper (create a person mapper and have above delegate to it)
    // https://eaflood.atlassian.net/wiki/spaces/MWR/pages/5595594864/ADR-122+RE+EX+Account+Creation+and+Enrolment
    // ^ that shows a post /api/v1/reprocessors-exporter-accounts accepting what looks like a full account with presumably blank (null) strings and notset enums, which doesn't seem right
    public PersonModel CreateReExAccountModel(AccountCreationSession session, string email)
    {
        //todo: session.Contact can be null
        var person = new PersonModel()
        {
            FirstName = session.Contact.FirstName,
            LastName = session.Contact.LastName,
            ContactEmail = email,
            TelephoneNumber = session.Contact.TelephoneNumber
        };

        return person;
    }

    private static AddressModel GetCompanyAddress(AccountCreationSession session)
    {
        var address = new AddressModel();
        
        if (session.IsCompaniesHouseFlow)
        {
            if (session.CompaniesHouseSession.Company.BusinessAddress is null)
            {
                return address;
            }

            address.SubBuildingName = session.CompaniesHouseSession.Company.BusinessAddress.SubBuildingName;
            address.BuildingName = session.CompaniesHouseSession.Company.BusinessAddress.BuildingName;
            address.BuildingNumber = session.CompaniesHouseSession.Company.BusinessAddress.BuildingNumber;
            address.Street = session.CompaniesHouseSession.Company.BusinessAddress.Street;
            address.Town = session.CompaniesHouseSession.Company.BusinessAddress.Town;
            address.Country = session.CompaniesHouseSession.Company.BusinessAddress.Country;
            address.Postcode = session.CompaniesHouseSession.Company.BusinessAddress.Postcode;
            address.Locality = session.CompaniesHouseSession.Company.BusinessAddress.Locality;
            address.DependentLocality = session.CompaniesHouseSession.Company.BusinessAddress.DependentLocality;
            address.County = session.CompaniesHouseSession.Company.BusinessAddress.County;
        }
        else
        {
            address.SubBuildingName = session.ManualInputSession.BusinessAddress.SubBuildingName;
            address.BuildingName = session.ManualInputSession.BusinessAddress.BuildingName;
            address.BuildingNumber = session.ManualInputSession.BusinessAddress.BuildingNumber;
            address.Street = session.ManualInputSession.BusinessAddress.Street;
            address.Town = session.ManualInputSession.BusinessAddress.Town;
            address.Country = session.ManualInputSession.BusinessAddress.Country;
            address.Postcode = session.ManualInputSession.BusinessAddress.Postcode;
            address.Locality = session.ManualInputSession.BusinessAddress.Locality;
            address.DependentLocality = session.ManualInputSession.BusinessAddress.DependentLocality;
            address.County = session.ManualInputSession.BusinessAddress.County;
        }

        return address;
    }
}
