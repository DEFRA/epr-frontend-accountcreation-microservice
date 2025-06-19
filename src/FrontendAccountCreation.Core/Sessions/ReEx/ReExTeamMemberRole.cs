using System.ComponentModel;

namespace FrontendAccountCreation.Core.Sessions.ReEx;

// Description decorator is used to map the value to a string, for saving to the database
public enum ReExTeamMemberRole
{
    None = 0,
    Director = 1,
    CompanySecretary = 2,

    [Description("Company director of a corporate partner")]
    PartnerDirector = 3,

    [Description("Company secretary of a corporate partner")]
    PartnerCompanySecretary = 4,
    IndividualPartner = 5,
    SoleTrader = 6,
}

    [Description("Individual partner")]
    IndividualPartner = 5,

    Member = 7
}