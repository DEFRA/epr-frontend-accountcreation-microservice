using System.Diagnostics.CodeAnalysis;
using FrontendAccountCreation.Core.Addresses;
using FrontendAccountCreation.Core.Sessions.ReEx.Partnership;

namespace FrontendAccountCreation.Core.Sessions.ReEx;

[ExcludeFromCodeCoverage]
public class ReExManualInputSession
{
    public string TradingName { get; set; }

    public string OrganisationName { get; set; }

    public string NonUkRoleInOrganisation { get; set; }

    // this will hold current user role in organisation it is set once irrespective
    // of flow can be in root session but that means refactoring all other flow
    public RoleInOrganisation? RoleInOrganisation { get; set; }

    /// <summary>
    /// Sets regulator's nation for the non-UK organisation.
    /// </summary>
    public Nation? UkRegulatorNation { get; set; }

    public ProducerType? ProducerType { get; set; }

    public Address? BusinessAddress { get; set; }
    // this is both in ReExCompaniesHouseSession and here. we could move it out of both, into the root session
    public bool? IsEligibleToBeApprovedPerson { get; set; }
  
    public List<ReExCompanyTeamMember>? TeamMembers { get; set; }
  
    // will be null unless ProducerType equals Partnership
    public ReExTypesOfPartner? TypesOfPartner { get; set; }
  
}
