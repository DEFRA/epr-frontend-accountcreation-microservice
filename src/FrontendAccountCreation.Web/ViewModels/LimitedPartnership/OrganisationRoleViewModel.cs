using FrontendAccountCreation.Core.Sessions.ReEx.LimitedPartnership;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.LimitedPartnership;

public class OrganisationRoleViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "RoleInOrganisation.ErrorMessage")]
    public TeamMemberRole? RoleInOrganisation { get; set; }
}
