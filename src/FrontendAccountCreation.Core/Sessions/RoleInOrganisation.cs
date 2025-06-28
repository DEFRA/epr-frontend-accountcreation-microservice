using System.ComponentModel;

namespace FrontendAccountCreation.Core.Sessions;

// Description decorator is used to map the value to a string, for saving to the database
public enum RoleInOrganisation
{
    NoneOfTheAbove = 0,

    [Description("Director")]
    Director = 1,

    [Description("CompanySecretary")]
    CompanySecretary = 2,

    [Description("Individual partner")]
    Partner = 3,

    [Description("Member")]
    Member = 4,

    [Description("Company director of a corporate partner")]
    PartnerDirector = 5,

    [Description("Company secretary of a corporate partner")]
    PartnerCompanySecretary = 6,

    [Description("Comnpany Director")]
    CompanyDirector = 7,
}