using FrontendAccountCreation.Core.Addresses;
using FrontendAccountCreation.Core.Extensions;
using FrontendAccountCreation.Core.Services.FacadeModels;
using FrontendAccountCreation.Core.Sessions;
using FrontendAccountCreation.Core.Sessions.ReEx;
using System;

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
        return new ReExOrganisationModel
        {
            UserRoleInOrganisation = reExOrganisationSession.ReExCompaniesHouseSession.RoleInOrganisation?.ToString() ?? null,
            IsApprovedUser = reExOrganisationSession.IsApprovedUser,
            Company = new ReExCompanyModel()
            {
                OrganisationId = reExOrganisationSession.ReExCompaniesHouseSession.Company?.OrganisationId ?? string.Empty,
                OrganisationType = reExOrganisationSession.OrganisationType?.ToString() ?? OrganisationType.NotSet.ToString(),
                ProducerType = GetProducerType(reExOrganisationSession),
                CompanyName = reExOrganisationSession.ReExCompaniesHouseSession.Company?.Name,
                CompaniesHouseNumber = reExOrganisationSession.ReExCompaniesHouseSession.Company?.CompaniesHouseNumber,
                CompanyRegisteredAddress = GetCompanyAddress(reExOrganisationSession.ReExCompaniesHouseSession.Company?.BusinessAddress),
                ValidatedWithCompaniesHouse = reExOrganisationSession.ReExCompaniesHouseSession.Company?.BusinessAddress is not null,
                Nation = reExOrganisationSession.UkNation ?? Nation.NotSet
            },
            InvitedApprovedPersons = GetTeamMembersModel(reExOrganisationSession.ReExCompaniesHouseSession.TeamMembers),
            Partners = GetPartnersModel(reExOrganisationSession)

        };
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

    private List<ReExPartnerModel>? GetPartnersModel(OrganisationSession reExOrganisationSession)
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

    private static List<ReExInvitedApprovedPerson> GetTeamMembersModel(IEnumerable<ReExCompanyTeamMember> teamMembers)
    {
        List<ReExInvitedApprovedPerson> approvedPeople = [];

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