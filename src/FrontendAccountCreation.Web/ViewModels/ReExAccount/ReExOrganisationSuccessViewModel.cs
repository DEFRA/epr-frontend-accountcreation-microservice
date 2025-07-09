using FrontendAccountCreation.Core.Sessions.ReEx;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class ReExOrganisationSuccessViewModel
{
    public string? CompanyName { get; set; }
    public List<ReExCompanyTeamMember> ReExCompanyTeamMembers { get; set; }

    public bool IsSoleTrader { get; set; }

    public bool IsCompaniesHouseFlow { get; set; }

    public bool? IsAnApprovedPerson { get; set; }
}
