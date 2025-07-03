using FrontendAccountCreation.Core.Sessions;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class NonCompaniesHousePartnershipRoleModel
{
    [Required(ErrorMessage = "NonCompaniesHousePartnershipYourRole.ErrorMessage")]
    public RoleInOrganisation? RoleInOrganisation { get; set; }
}
