using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class AccountPersonViewModel
{
    [Required(ErrorMessage = "IsOptionSelected.ErrorMessage")]
    public bool IsApprovedPersonOptionSelected => AgreedApprovedPerson.HasValue || InviteEligiblePerson.HasValue || InviteApprovedPersonLater.HasValue;

    public bool? AgreedApprovedPerson { get; set; }

    public bool? InviteEligiblePerson { get; set; }

    public bool? InviteApprovedPersonLater { get; set; }
}
