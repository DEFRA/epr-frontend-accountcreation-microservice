using FrontendAccountCreation.Web.ViewModels.Shared.GovUK;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class NonCompaniesHousePartnershipInviteApprovedPersonViewModel
{
    [Required]
    public string InviteUserOption { get; set; }
    public ErrorsViewModel? ErrorsViewModel { get; set; }
    public bool IsNonCompaniesHousePartnership { get; set; }
}

