using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

[ExcludeFromCodeCoverage]
public class NonCompaniesHousePartnershipInviteApprovedPersonViewModel
{
    [Required]
    public string InviteUserOption { get; set; }
}

