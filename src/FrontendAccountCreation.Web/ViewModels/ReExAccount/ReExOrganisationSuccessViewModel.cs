using FrontendAccountCreation.Core.Sessions.ReEx;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class ReExOrganisationSuccessViewModel
{
    public string? CompanyName { get; set; }
    public List<ReExCompanyTeamMember> reExCompanyTeamMembers { get; set; }
}
