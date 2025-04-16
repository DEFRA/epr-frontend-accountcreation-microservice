using FrontendAccountCreation.Core.Services.Dto.Company;

namespace FrontendAccountCreation.Core.Sessions;

public class CompaniesHouseSession
{
    public Company Company { get; set; }
    
    public RoleInOrganisation? RoleInOrganisation { get; set; }
    
    public bool IsComplianceScheme { get; set; }

    public TeamMemberRoleInOrganisation? TeamMemberRoleInOrganisation { get; set; }
}
