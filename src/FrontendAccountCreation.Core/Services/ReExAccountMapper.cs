using FrontendAccountCreation.Core.Addresses;
using FrontendAccountCreation.Core.Services.FacadeModels;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;

namespace FrontendAccountCreation.Core.Services;

public class ReExAccountMapper : IReExAccountMapper
{
    public ReprocessorExporterAccountModel CreateReprocessorExporterAccountModel(ReExAccountCreationSession session, string email)
    {
        return new ReprocessorExporterAccountModel
        {
            Person = new PersonModel
            {
                FirstName = session.Contact.FirstName,
                LastName = session.Contact.LastName,
                ContactEmail = email,
                TelephoneNumber = session.Contact.TelephoneNumber
            }
        };
    }

    public ReExOrganisationModel CreateReExOrganisationModel(OrganisationSession reExOrganisationSession)
    {
        return new ReExOrganisationModel()
        {
            CompanyName = reExOrganisationSession.ReExCompaniesHouseSession.Company.Name,
            CompaniesHouseNumber = reExOrganisationSession.ReExCompaniesHouseSession.Company.CompaniesHouseNumber,
            CompanyAddress = GetCompanyAddress(reExOrganisationSession.ReExCompaniesHouseSession.Company.BusinessAddress),
            ValidatedWithCompaniesHouse = reExOrganisationSession.ReExCompaniesHouseSession.Company.BusinessAddress is not null,
            OrganisationType = reExOrganisationSession.OrganisationType,
            Nation = reExOrganisationSession.UkNation ?? Nation.NotSet,
            OrganisationId = reExOrganisationSession.ReExCompaniesHouseSession.Company.OrganisationId
        };
    }

    /// <summary>
    /// Gets company address
    /// </summary>
    /// <param name="address"></param>
    /// <returns>mapped company address</returns>
    private static AddressModel? GetCompanyAddress(Address address)
    {
        return address is null ? null : new AddressModel()
        {
            BuildingName = address.BuildingName,
            BuildingNumber = address.BuildingNumber,
            Street = address.Street,
            Town = address.Town,
            Country = address.Country,
            Postcode = address.Postcode,
            Locality = address.Locality,
            DependentLocality = address.DependentLocality,
            County = address.County
        };
    }
}