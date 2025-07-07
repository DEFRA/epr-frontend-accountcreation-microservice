using FrontendAccountCreation.Core.Sessions.ReEx;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount
{
    [ExcludeFromCodeCoverage]
    public class NonCompaniesHouseTeamMemberCheckInvitationDetailsViewModel
    {
        public bool IsSoleTrader { get; set; }

        public bool IsNonUk { get; set; }

        public bool IsPartnership { get; set; }
        
        public List<ReExCompanyTeamMember>? TeamMembers { get; set; }
    }
}

