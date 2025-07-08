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
            UserRoleInOrganisation = GetUserRole(session),
            IsApprovedUser = session.IsApprovedUser,
            Company = session.ReExCompaniesHouseSession != null ? GetCompanyModel(session) : null,
            ManualInput = session.ReExManualInputSession != null ? GetManualInputModel(session) : null,
            InvitedApprovedPersons = GetTeamMembersModel(session),
            Partners = GetPartnersModel(session),
            TradingName = session.TradingName
        };
    }

    private static string? GetUserRole(OrganisationSession session)
    {
        if (session.IsCompaniesHouseFlow || session.ReExCompaniesHouseSession != null)
        {
            return session.ReExCompaniesHouseSession?.RoleInOrganisation?.GetDescriptionOrNull();
        }
        else
        {
            if (session.ReExManualInputSession?.ProducerType == ProducerType.NonUkOrganisation)
            {
                return session.ReExManualInputSession.NonUkRoleInOrganisation;
            }
            if (session.ReExManualInputSession?.ProducerType == ProducerType.Partnership)
            {
                return session.ReExManualInputSession?.RoleInOrganisation?.GetDescriptionOrNull();
            }
            if (session.ReExManualInputSession?.ProducerType == ProducerType.UnincorporatedBody)
            {
                return session.ReExManualInputSession.RoleInUnincorporatedOrganisation;
            }
        }
        return null;
    }

    private static ReExManualInputModel GetManualInputModel(OrganisationSession session)
    {
        return new ReExManualInputModel()
        {
            BusinessAddress = GetAddressModel(session.ReExManualInputSession?.BusinessAddress),
            ProducerType = session.ReExManualInputSession?.ProducerType,
            Nation = GetNation(session),
            OrganisationType = session.OrganisationType ?? OrganisationType.NotSet,
            OrganisationName = session.ReExManualInputSession?.OrganisationName,
            NonUkRoleInOrganisation = session.ReExManualInputSession?.NonUkRoleInOrganisation
        };
    }
    private static Nation GetNation(OrganisationSession session)
    {
        if (session.ReExManualInputSession is not null && session.ReExManualInputSession?.UkRegulatorNation is not null)
        {
            return session.ReExManualInputSession.UkRegulatorNation.Value;
        }
        else
        {
           return session.UkNation ?? Nation.NotSet;
        }             
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

    private static List<ReExInvitedApprovedPerson> GetTeamMembersModel(OrganisationSession session)
    {
        List<ReExCompanyTeamMember> teamMembers;

        if (session.ReExManualInputSession != null && session.ReExManualInputSession.TeamMembers != null)
        {
            teamMembers = session.ReExManualInputSession.TeamMembers;
        }
        else
        {
            // If we are in Companies House flow, we get team members from the Companies House session
            teamMembers = session.ReExCompaniesHouseSession?.TeamMembers ?? [];
        }

        List<ReExInvitedApprovedPerson> approvedPeople = new List<ReExInvitedApprovedPerson>();

        foreach (var member in teamMembers)
        {
            var memberModel = new ReExInvitedApprovedPerson()
            {
                Id = member.Id,
                FirstName = member.FirstName,
                LastName = member.LastName,
                Email = member.Email,
                Role = member.Role?.GetDescriptionOrNull(),
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
        List<Web.ViewModels.ReExAccount.ReExPersonOrCompanyPartner>? partners = null;

        if (reExOrganisationSession.IsCompaniesHouseFlow)
        {
            partners = reExOrganisationSession.ReExCompaniesHouseSession?.Partnership?.LimitedPartnership?.Partners;
        }
        else
        {
            partners = reExOrganisationSession.ReExManualInputSession?.TypesOfPartner?.Partners;
        }

        if (partners == null || partners.Count == 0)
        {
            return null;
        }

        return [.. partners.Select(x => new ReExPartnerModel
        {
            Name = x.Name,
            PartnerRole = x.IsPerson
                ? PartnerType.IndividualPartner.GetDescription()
                : PartnerType.CorporatePartner.GetDescription()
        })];
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