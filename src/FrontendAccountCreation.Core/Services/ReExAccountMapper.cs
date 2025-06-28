using FrontendAccountCreation.Core.Addresses;
using FrontendAccountCreation.Core.Extensions;
using FrontendAccountCreation.Core.Services.Dto.Company;
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
            UserRoleInOrganisation = session.ReExCompaniesHouseSession?.RoleInOrganisation?.GetDescriptionOrNull() ?? null,
            IsApprovedUser = session.IsApprovedUser,
            Company = session.ReExCompaniesHouseSession != null ? GetCompanyModel(session) : null,
            ManualInput = session.ReExManualInputSession != null ? GetManualInputModel(session) : null,
            InvitedApprovedPersons = GetTeamMembersModel(session.ReExCompaniesHouseSession?.TeamMembers, session.ReExManualInputSession),
            Partners = GetPartnersModel(session)
        };
    }

    private static ReExManualInputModel GetManualInputModel(OrganisationSession session)
    {
        return new ReExManualInputModel()
        {
            BusinessAddress = GetAddressModel(session.ReExManualInputSession?.BusinessAddress),
            ProducerType = session.ReExManualInputSession?.ProducerType,
            TradingName = session.ReExManualInputSession?.TradingName,
            Nation = session.UkNation ?? Nation.NotSet,
            OrganisationType = session.OrganisationType ?? OrganisationType.NotSet
        };
    }

    private static ReExCompanyModel GetCompanyModel(OrganisationSession session)
    {
        return new ReExCompanyModel()
        {
            OrganisationId = session.ReExCompaniesHouseSession?.Company?.OrganisationId ?? string.Empty,
            OrganisationType = session.OrganisationType ?? OrganisationType.NotSet,
            ProducerType = GetProducerType(session),
            CompanyName = session.ReExCompaniesHouseSession?.Company?.Name,
            CompaniesHouseNumber = session.ReExCompaniesHouseSession?.Company?.CompaniesHouseNumber,
            CompanyRegisteredAddress = GetAddressModel(session.ReExCompaniesHouseSession?.Company?.BusinessAddress),
            ValidatedWithCompaniesHouse = session.ReExCompaniesHouseSession?.Company?.BusinessAddress is not null,
            Nation = session.UkNation ?? Nation.NotSet,
            IsComplianceScheme = session.ReExCompaniesHouseSession?.IsComplianceScheme ?? false
        };
    }

    private static List<ReExInvitedApprovedPerson> GetTeamMembersModel(IEnumerable<ReExCompanyTeamMember>? teamMembers = null, ReExManualInputSession? reExManualInput = null)
    {
        List<ReExInvitedApprovedPerson> approvedPeople = [];

        if (reExManualInput != null && reExManualInput.TeamMembers != null)
        {
            var teamMember = reExManualInput.TeamMembers[0];
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
                Role = member.Role?.GetDescriptionOrNull() ?? null,
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

    private static List<ReExPartnerModel>? GetPartnersModel(OrganisationSession reExOrganisationSession)
    {
        List<ReExPartnerModel>? reExPartnerModels = null;
        var partners = reExOrganisationSession?.ReExCompaniesHouseSession?.Partnership?.LimitedPartnership?.Partners?.ToList();
        if (partners is { Count: > 0 })
        {
            reExPartnerModels = [.. partners.Select(x => new ReExPartnerModel()
            {
                Name = x.Name,
                PartnerRole = x.IsPerson ? PartnerType.IndividualPartner.GetDescription() : PartnerType.CorporatePartner.GetDescription(),
            })];
        }
        return reExPartnerModels;
    }

    private static string? GetProducerType(OrganisationSession reExOrganisationSession)
    {
        var producerType = ProducerType.NotSet.ToString();
        if (reExOrganisationSession.IsCompaniesHouseFlow)
        {
            producerType = reExOrganisationSession.ReExCompaniesHouseSession.ProducerType?.ToString() ?? ProducerType.NotSet.ToString();
        }
        return producerType;
    }
}