using System.ComponentModel;

namespace FrontendAccountCreation.Core.Sessions;

// Description decorator is used to map the value to a string, for saving to the database
public enum RoleInOrganisation
{
    NoneOfTheAbove = 0,

    [Description("Company director of a corporate partner")]
    Director = 1,

    [Description("Company secretary of a corporate partner")]
    CompanySecretary = 2,

    [Description("Individual partner")]
    Partner = 3,

    Member = 4
}