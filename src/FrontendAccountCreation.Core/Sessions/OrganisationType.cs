using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Core.Sessions;

public enum OrganisationType
{
    NotSet = 0,
    CompaniesHouseCompany = 1,
    NonCompaniesHouseCompany = 2,

    [Display(Name = "Unincorporated association")]
    UnincorporatedAssociation = 3
}
