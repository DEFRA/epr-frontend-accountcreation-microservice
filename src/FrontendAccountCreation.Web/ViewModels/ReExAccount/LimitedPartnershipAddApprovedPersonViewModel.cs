using System.ComponentModel.DataAnnotations;

namespace FrontendAccountCreation.Web.ViewModels.ReExAccount
{
    public class LimitedPartnershipAddApprovedPersonViewModel
    {
        [Required(ErrorMessage = "AddAnApprovedPerson.OptionError")]
        public string InviteUserOption { get; set; }
    }
}
