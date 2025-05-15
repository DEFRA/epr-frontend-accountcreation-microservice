using FrontendAccountCreation.Core.Services.Dto.Company;
using FrontendAccountCreation.Core.Sessions.ReEx.Partnership;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Core.Sessions.ReEx;

[ExcludeFromCodeCoverage(Justification = "Get feature branch into testing")]
public class ReExCompaniesHouseSession
{
    public Company Company { get; set; }

    public RoleInOrganisation? RoleInOrganisation { get; set; }

    public bool IsComplianceScheme { get; set; }

    public List<ReExCompanyTeamMember>? TeamMembers { get; set; }

    public bool? IsPartnership { get; set; }

    public ReExPartnership? Partnership { get; set; }

    public bool IsIneligible { get; set; }
}
