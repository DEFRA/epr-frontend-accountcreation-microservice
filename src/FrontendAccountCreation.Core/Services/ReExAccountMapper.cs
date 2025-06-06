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

    public ReExOrganisationModel CreateReExOrganisationModel(OrganisationSession session)
    {
        return new ReExOrganisationModel
        {
            UserRoleInOrganisation = session.ReExCompaniesHouseSession?.RoleInOrganisation?.ToString() ?? null,
            IsApprovedUser = session.IsApprovedUser,
            Company = session.ReExCompaniesHouseSession == null ? null : GetCompanyModel(session),
            ManualInput = session.ReExManualInputSession == null ? null : GetManualInputModel(session),
            InvitedApprovedPersons = GetTeamMembersModel(session.ReExCompaniesHouseSession?.TeamMembers, session.ReExManualInputSession)
        };
    }

    private static ReExManualInputModel GetManualInputModel(OrganisationSession session)
    {
        return new ReExManualInputModel()
        {
            BusinessAddress = GetAddressModel(session.ReExManualInputSession?.BusinessAddress),
            ProducerType = session.ReExManualInputSession?.ProducerType,
            TradingName = session.ReExManualInputSession?.TradingName
        };
    }

    private static ReExCompanyModel GetCompanyModel(OrganisationSession session)
    {
        return new ReExCompanyModel()
        {
            OrganisationId = session.ReExCompaniesHouseSession?.Company?.OrganisationId ?? string.Empty,
            OrganisationType = session.OrganisationType?.ToString() ?? OrganisationType.NotSet.ToString(),
            CompanyName = session.ReExCompaniesHouseSession?.Company?.Name,
            CompaniesHouseNumber = session.ReExCompaniesHouseSession?.Company?.CompaniesHouseNumber,
            CompanyRegisteredAddress = GetAddressModel(session.ReExCompaniesHouseSession?.Company?.BusinessAddress),
            ValidatedWithCompaniesHouse = session.ReExCompaniesHouseSession?.Company?.BusinessAddress is not null,
            Nation = session.UkNation ?? Nation.NotSet
        };
    }

    private static List<ReExInvitedApprovedPerson> GetTeamMembersModel(IEnumerable<ReExCompanyTeamMember>? teamMembers = null, ReExManualInputSession? reExManualInput = null)
    {
        List<ReExInvitedApprovedPerson> approvedPeople = [];

        if (reExManualInput != null && reExManualInput.TeamMember != null)
        {
            var teamMember = reExManualInput.TeamMember;
            approvedPeople.Add(new ReExInvitedApprovedPerson
            {
                Email = teamMember.Email,
                FirstName = teamMember.FirstName,
                Id = teamMember.Id,
                LastName = teamMember.LastName,
                Role = teamMember.Role?.ToString() ?? null,
                TelephoneNumber = teamMember.TelephoneNumber

            });
            return approvedPeople;
        }

        foreach (var member in teamMembers ?? [])
        {
            var memberModel = new ReExInvitedApprovedPerson()
            {
                Id = member.Id,
                FirstName = member.FirstName,
                LastName = member.LastName,
                Email = member.Email,
                Role = member.Role?.ToString() ?? null,
                TelephoneNumber = member.TelephoneNumber
            };
            approvedPeople.Add(memberModel);
        }
        return approvedPeople;
    }

    /// <summary>
    /// Gets address model
    /// </summary>
    /// <returns>mapped model</returns>
    private static AddressModel? GetAddressModel(Address address)
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