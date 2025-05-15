using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount
{
    [ExcludeFromCodeCoverage(Justification = "Get feature branch into testing")]
    public class LimitedPartnershipAddApprovedPersonViewModel
    {
        [Required(ErrorMessage = "AddAnApprovedPerson.OptionError")]
        public string InviteUserOption { get; set; }
    }
}
