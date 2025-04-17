namespace FrontendAccountCreation.Core.Sessions;

public class CompanyTeamMember
{
    public string FullName { get; set; }

    public string Email  { get; set; }

    public string Telephone { get; set; }

    public TeamMemberRoleInOrganisation? Role { get; set; }
}
