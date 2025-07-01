using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Core.Sessions.ReEx;

[ExcludeFromCodeCoverage]
public class ReExUnincorporatedFlowSession
{
    public string? RoleInOrganisation { get; set; }

    public ManageControlAnswer ManageControlAnswer { get; set; }

    public ManageAccountPersonAnswer ManageAccountPersonAnswer { get; set; }

    public ManageControlAnswer ManageControlOrganisationAnswer { get; set; }

    public IDictionary<Guid, ReExCompanyTeamMember>? TeamMemberDetailsDictionary { get; set; }
}