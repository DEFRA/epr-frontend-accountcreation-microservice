using FrontendAccountCreation.Core.Sessions;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class NonCompaniesHousePartnershipRoleModel
{
    [Required(ErrorMessage = "NonCompaniesHousePartnershipRole.ErrorMessage")]
    public RoleInOrganisation? RoleInOrganisation { get; set; }
}
