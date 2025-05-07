using FrontendAccountCreation.Core.Services.Dto.Company;

namespace FrontendAccountCreation.Core.Sessions.ReEx;

public class ReExCompaniesHouseSession
{
    public Company Company { get; set; }

    public RoleInOrganisation? RoleInOrganisation { get; set; }

    public bool IsComplianceScheme { get; set; }

    public List<ReExCompanyTeamMember?> TeamMembers { get; set; }
}
