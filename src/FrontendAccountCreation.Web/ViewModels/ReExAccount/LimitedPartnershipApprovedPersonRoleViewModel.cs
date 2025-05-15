using FrontendAccountCreation.Core.Sessions.ReEx;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

[ExcludeFromCodeCoverage(Justification = "Get feature branch into testing")]
public class LimitedPartnershipApprovedPersonRoleViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "PartnershipApprovedPersonRole.ErrorMessage")]
    public ReExTeamMemberRole? RoleInOrganisation { get; set; }
}
