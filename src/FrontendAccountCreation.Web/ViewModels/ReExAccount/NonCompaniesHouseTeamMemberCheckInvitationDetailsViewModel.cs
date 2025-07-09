using FrontendAccountCreation.Core.Sessions.ReEx;
using Microsoft.AspNetCore.Mvc.Localization;
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

        public string GetLocalizedRole(ReExTeamMemberRole? role, IViewLocalizer localizer)
        {
            return role switch
            {
                ReExTeamMemberRole.IndividualPartner => localizer["NonCompaniesHouseTeamMemberCheckInvitationDetails.IndividualPartner"].Value,
                ReExTeamMemberRole.PartnerDirector => localizer["NonCompaniesHouseTeamMemberCheckInvitationDetails.CompanyDirector"].Value,
                ReExTeamMemberRole.PartnerCompanySecretary => localizer["NonCompaniesHouseTeamMemberCheckInvitationDetails.CompanySecretary"].Value,
                ReExTeamMemberRole.None => localizer["NonCompaniesHouseTeamMemberCheckInvitationDetails.None"].Value
            };
        }
    }
}

