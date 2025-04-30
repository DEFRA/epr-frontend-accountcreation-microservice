using System;
using FrontendAccountCreation.Core.Services.FacadeModels;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;

namespace FrontendAccountCreation.Core.Services;

public class OrganisationMapper : IOrganisationMapper
{
    private const string ApprovedPersonServiceRole = "Packaging.ApprovedPerson";

    public AccountModel CreateOrganisationModel(OrganisationSession session, string email)
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
            name = session.ReExCompaniesHouseSession.Company.Name;
            companiesHouseNumber = session.ReExCompaniesHouseSession.Company.CompaniesHouseNumber;
            isComplianceScheme = session.ReExCompaniesHouseSession.IsComplianceScheme;
            validatedWithCompaniesHouse = true;
            jobTitle = session.ReExCompaniesHouseSession.RoleInOrganisation.ToString();
            producerType = null;
            organisationId = session.ReExCompaniesHouseSession.Company.OrganisationId;
        }
        else
        {
            name = session.ReExManualInputSession.TradingName;
            companiesHouseNumber = null;
            isComplianceScheme = false;
            validatedWithCompaniesHouse = false;
            jobTitle = session.ReExManualInputSession.RoleInOrganisation;
            producerType = session.ReExManualInputSession.ProducerType;
            organisationId = session.ReExManualInputSession.OrganisationId;
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

    private static AddressModel GetCompanyAddress(OrganisationSession session)
    {
        var address = new AddressModel();

        if (session.IsCompaniesHouseFlow)
        {
            if (session.ReExCompaniesHouseSession.Company.BusinessAddress is null)
            {
                return address;
            }

            address.SubBuildingName = session.ReExCompaniesHouseSession.Company.BusinessAddress.SubBuildingName;
            address.BuildingName = session.ReExCompaniesHouseSession.Company.BusinessAddress.BuildingName;
            address.BuildingNumber = session.ReExCompaniesHouseSession.Company.BusinessAddress.BuildingNumber;
            address.Street = session.ReExCompaniesHouseSession.Company.BusinessAddress.Street;
            address.Town = session.ReExCompaniesHouseSession.Company.BusinessAddress.Town;
            address.Country = session.ReExCompaniesHouseSession.Company.BusinessAddress.Country;
            address.Postcode = session.ReExCompaniesHouseSession.Company.BusinessAddress.Postcode;
            address.Locality = session.ReExCompaniesHouseSession.Company.BusinessAddress.Locality;
            address.DependentLocality = session.ReExCompaniesHouseSession.Company.BusinessAddress.DependentLocality;
            address.County = session.ReExCompaniesHouseSession.Company.BusinessAddress.County;
        }
        else
        {
            address.SubBuildingName = session.ReExManualInputSession.BusinessAddress.SubBuildingName;
            address.BuildingName = session.ReExManualInputSession.BusinessAddress.BuildingName;
            address.BuildingNumber = session.ReExManualInputSession.BusinessAddress.BuildingNumber;
            address.Street = session.ReExManualInputSession.BusinessAddress.Street;
            address.Town = session.ReExManualInputSession.BusinessAddress.Town;
            address.Country = session.ReExManualInputSession.BusinessAddress.Country;
            address.Postcode = session.ReExManualInputSession.BusinessAddress.Postcode;
            address.Locality = session.ReExManualInputSession.BusinessAddress.Locality;
            address.DependentLocality = session.ReExManualInputSession.BusinessAddress.DependentLocality;
            address.County = session.ReExManualInputSession.BusinessAddress.County;
        }

        return address;
    }
}