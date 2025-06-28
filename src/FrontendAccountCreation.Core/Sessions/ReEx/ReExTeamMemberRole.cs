using System.ComponentModel;

namespace FrontendAccountCreation.Core.Sessions.ReEx;

// Description decorator is used to map the value to a string, for saving to the database
public enum ReExTeamMemberRole
{
    None = 0,

    [Description("Director")]
    Director = 1,

    [Description("CompanySecretary")]
    CompanySecretary = 2,

    [Description("Company director of a corporate partner")]
    PartnerDirector = 3,

    [Description("Company secretary of a corporate partner")]
    PartnerCompanySecretary = 4,

    [Description("Individual partner")]
    IndividualPartner = 5,

    SoleTrader = 6,

    [Description("Member")]
    Member = 7,

    [Description("Company Director")]
    CompanyDirector = 8,
}