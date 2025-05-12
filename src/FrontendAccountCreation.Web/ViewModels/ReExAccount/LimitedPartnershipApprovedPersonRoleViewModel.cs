using FrontendAccountCreation.Core.Sessions.ReEx.Partnership.ApprovedPersons;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class LimitedPartnershipApprovedPersonRoleViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "PartnershipApprovedPersonRole.ErrorMessage")]
    public ReExLimitedPartnershipRoles? RoleInOrganisation { get; set; }
}
