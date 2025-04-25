using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount;

public class AccountPersonViewModel
{
    [Required(ErrorMessage = "IsApprovedPersonOptionSelected.ErrorMessage")]
    public bool IsApprovedPersonOptionSelected => AgreedApprovedPerson.HasValue || InviteEligiblePerson.HasValue || InviteApprovedPersonLater.HasValue;

    public YesNoAnswer? AgreedApprovedPerson { get; set; }

    public YesNoAnswer? InviteEligiblePerson { get; set; }

    public YesNoAnswer? InviteApprovedPersonLater { get; set; }
}
